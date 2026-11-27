#!/usr/bin/env bash
# studio-hook.sh — the ONLY script in this repo. Claude Code runs it from hooks declared in
# .claude/settings.json and in .claude/agents/*.md frontmatter. The hook's JSON payload arrives on stdin.
# Works in Git Bash (Windows), Linux and macOS (bash 3.2+). No jq: values are pulled out with grep/sed,
# conservatively. When in doubt, it blocks.
#
# Exit codes: 0 = allow / ok. 2 = block; Claude sees the stderr text as the reason.
#
# MODES
#   bash-guard
#       PreToolUse on Bash. Reads tool_input.command (falls back to the whole payload) and blocks:
#       - git push with --force/-f/--force-with-lease/--mirror/--delete (or a +refspec), a push that
#         targets main/master, or any push while the current branch is main/master;
#       - gh pr merge, gh api, gh auth, gh secret, gh variable, gh workflow, gh ruleset,
#         gh repo edit|delete|rename|archive, gh release delete|edit, gh run rerun|cancel,
#         gh extension install;
#       - git config --global|--system, git remote add|set-url|remove|rename, --no-verify;
#       - curl/wget (or PowerShell web cmdlets) calls to api.github.com;
#       - starting a claude process, --dangerously-skip-permissions, --setting-sources, disableAllHooks;
#       - WRITES to protected paths (redirects, tee, sed -i, perl -i, mv, cp, rm, touch, chmod, ln,
#         git checkout --/restore/rm, inline python/node/dotnet code mentioning them). Reading is allowed;
#       - git push or any gh command from a SUBAGENT (non-empty agent_type): only the Producer talks
#         to GitHub.
#   path-guard <prefix...>
#       PreToolUse on Edit|Write, declared per agent. Reads tool_input.file_path, normalizes it to a
#       project-relative path (backslashes, D:/ vs /d/ forms, case-insensitive). Allows the edit only if
#       the path starts with one of the prefixes (a prefix ending in "/" is a folder; otherwise an exact
#       file). Protected paths and paths outside the project are always denied.
#   session-context
#       SessionStart. Prints at most 25 lines that Claude Code adds to the session context: branch, last
#       commit, task log/plan or setup progress, open PRs, and a hint about /resume-task and /inbox.
#       If the session's model is not Sonnet (from the optional "model" field, else the --model flag of
#       the parent claude process, as the desktop app omits the field), it prints JSON instead: a
#       systemMessage warning for the client plus the same text as additionalContext for the Producer.
#   build-gate
#       Stop hook of the developer agent (runs as SubagentStop). Runs
#       `dotnet build Boxing.sln -warnaserror -nologo -v q`. On failure: last 40 lines to stderr, exit 2
#       (the agent keeps working). If stop_hook_active is already true and it still fails: print
#       "BUILD STILL FAILING — report STATUS: BLOCKED" and exit 0 so the agent can stop.
#
# PROTECTED PATHS (agents never write): .claude/ .github/ CLAUDE.md docs/process/ global.json
#                                       .gitignore .gitattributes
set -u

MODE="${1:-}"
[ $# -gt 0 ] && shift

PAYLOAD=""
if [ ! -t 0 ]; then PAYLOAD="$(cat)"; fi

SQ="'"
# Characters that may sit right before / after a command word or a path.
B_START="(^|[[:space:]\"${SQ}\`;(|&/])"
B_END="([[:space:]\"${SQ}\`;)|&]|\$)"
# Protected paths as they may appear inside a shell command (matched case-insensitively).
PROT_CMD="(\\.claude([/[:space:]\"${SQ}]|\$)|\\.github([/[:space:]\"${SQ}]|\$)|claude\\.md|docs/process|global\\.json|\\.gitignore|\\.gitattributes)"
# Protected paths relative to the project root (lower-cased before matching).
PROT_REL='^(\.claude(/|$)|\.github(/|$)|docs/process(/|$)|claude\.md$|global\.json$|\.gitignore$|\.gitattributes$)'

lower() { printf '%s' "$1" | tr '[:upper:]' '[:lower:]'; }

# Matching uses bash's built-in [[ =~ ]] (POSIX ERE): no extra processes, which keeps the guard fast on
# Windows, where every grep/sed costs tens of milliseconds.
has()   { [[ $2 =~ $1 ]]; }
has_i() { local r; shopt -s nocasematch; [[ $2 =~ $1 ]]; r=$?; shopt -u nocasematch; return $r; }

# JV = first string value of "key" anywhere in the payload, JSON-unescaped (best effort:
# \\ \" \/ \n \r \t and the \u escapes for < > & '). Returns 1 if the key is absent.
JV=""
json_get() {
  local flat re v bs=$'\001'
  JV=""
  flat="${PAYLOAD//$'\r'/ }"; flat="${flat//$'\n'/ }"
  re="\"$1\"[[:space:]]*:[[:space:]]*\"(([^\"\\\\]|\\\\.)*)\""
  [[ $flat =~ $re ]] || return 1
  v="${BASH_REMATCH[1]}"
  v="${v//\\\\/$bs}"
  v="${v//\\\"/\"}"
  v="${v//\\\//\/}"
  v="${v//\\n/ ; }"; v="${v//\\r/ ; }"; v="${v//\\t/ }"
  v="${v//\\u003e/>}"; v="${v//\\u003E/>}"; v="${v//\\u003c/<}"; v="${v//\\u003C/<}"
  v="${v//\\u0026/&}"; v="${v//\\u0027/$SQ}"
  JV="${v//$bs/\\}"
}

# PD = project directory with forward slashes and no trailing slash.
PD=""
set_project_dir() { PD="${CLAUDE_PROJECT_DIR:-$PWD}"; PD="${PD//\\//}"; PD="${PD%/}"; }

# Run a command with a time limit (timeout/gtimeout, or a background kill as a fallback).
run_timeout() {
  local secs="$1"; shift
  if command -v timeout >/dev/null 2>&1; then timeout "$secs" "$@"; return $?; fi
  if command -v gtimeout >/dev/null 2>&1; then gtimeout "$secs" "$@"; return $?; fi
  "$@" & local pid=$!
  ( sleep "$secs"; kill "$pid" 2>/dev/null ) & local killer=$!
  wait "$pid"; local rc=$?
  kill "$killer" 2>/dev/null
  return $rc
}

block() {
  printf 'BLOCKED by studio-hook %s: %s\n' "$MODE" "$1" >&2
  printf 'This action is reserved for the client. Stop and report it (ESCALATE or QUESTIONS), or for a protected file write work/<n>/governance-proposal.md.\n' >&2
  exit 2
}

# SEGS = the command split into simple segments. Fine split: && || ; | $( ` ( ) { } and newlines.
# Coarse split (2nd arg "coarse"): only && || ; | and newlines, which keeps python -c "open(...)" whole.
SEGS=()
split_segments() {
  local s="$1" line nl=$'\n'
  s="${s//&&/$nl}"; s="${s//||/$nl}"; s="${s//;/$nl}"; s="${s//|/$nl}"
  if [ "${2:-}" != coarse ]; then
    s="${s//\$(/$nl}"; s="${s//\`/$nl}"; s="${s//(/$nl}"; s="${s//)/$nl}"; s="${s//\{/$nl}"; s="${s//\}/$nl}"
  fi
  SEGS=()
  while IFS= read -r line; do
    line="${line#"${line%%[![:space:]]*}"}"
    [ -n "$line" ] && SEGS[${#SEGS[@]}]="$line"
  done <<SEGEOF
$s
SEGEOF
}

# SW = segment without leading VAR=value assignments and wrapper words (sudo, env, exec, ...).
SW=""
WRAP_RE='^(sudo|env|exec|command|nohup|time|builtin|xargs|[A-Za-z_][A-Za-z0-9_]*=[^[:space:]]*)[[:space:]]+(.*)$'
strip_wrappers() {
  SW="$1"
  while [[ $SW =~ $WRAP_RE ]]; do SW="${BASH_REMATCH[2]}"; done
}

GIT_WORD="${B_START}git(\\.exe)?"
PUSH_RE="${GIT_WORD}([[:space:]]+(-C|-c|--git-dir|--work-tree)[[:space:]]+[^[:space:]]+|[[:space:]]+--?[[:alnum:]-]+(=[^[:space:]]*)?)*[[:space:]]+push${B_END}"
GH_ADMIN_RE="${B_START}gh(\\.exe)?[[:space:]]+(pr[[:space:]]+merge|api|auth|secret|variable|workflow|ruleset|repo[[:space:]]+(edit|delete|rename|archive)|release[[:space:]]+(delete|edit)|run[[:space:]]+(rerun|cancel)|extension[[:space:]]+install)${B_END}"
GH_ANY_RE="${B_START}gh(\\.exe)?([[:space:]]|\$)"
WRITE_WORD_RE="${B_START}(tee|mv|cp|rm|rmdir|touch|chmod|chown|ln|truncate|dd|install|unlink|mkdir|patch)(\\.exe)?${B_END}"
INPLACE_RE="${B_START}(sed|perl|ruby)(\\.exe)?[[:space:]](.*[[:space:]])?(-[[:alnum:]]*i|--in-place)"
GIT_WRITE_RE="${GIT_WORD}[[:space:]](.*[[:space:]])?(checkout|restore|rm|mv|apply|reset|clean|stash)${B_END}"
INTERP_RE="${B_START}(python[0-9.]*|py|node|deno|bun|dotnet|pwsh|powershell|ruby|perl|php|cmd)(\\.exe)?${B_END}"
REDIRECT_PROT_RE=">>?\\|?[[:space:]]*[\"${SQ}]?[^[:space:]\"${SQ}]*${PROT_CMD}"

PUSH_ARGS_RE='^.*[[:space:]]push(([[:space:]].*)?)$'

check_segment() {
  local s
  strip_wrappers "$1"; s="$SW"

  # A nested claude process (at the start of a segment, or inside sh -c / npx).
  if has "^[\"${SQ}]?([^[:space:]\"${SQ}]*/)?claude(\\.exe|\\.cmd)?[\"${SQ}]?([[:space:]]|\$)" "$s" \
     || has "-(c|Command)[[:space:]]+[\"${SQ}]?claude([[:space:]]|\$)" "$s" \
     || has "@anthropic-ai/claude-code|npx[[:space:]]+([^[:space:]]+[[:space:]]+)*claude([[:space:]]|\$)" "$s"; then
    block "starting a claude process is not allowed"
  fi

  if has "$GH_ADMIN_RE" "$s"; then
    block "this gh command is reserved for the client (merge, api, auth, secrets, variables, workflows, rulesets, repo/release admin, run rerun/cancel, extensions)"
  fi

  if has "$GIT_WORD[[:space:]]" "$s"; then
    if has "[[:space:]]config([[:space:]]|\$)" "$s" && has "[[:space:]]--(global|system)${B_END}" "$s"; then
      block "git config --global/--system changes machine-wide settings"
    fi
    if has "[[:space:]]remote[[:space:]]+(add|set-url|remove|rm|rename)${B_END}" "$s"; then
      block "changing git remotes is not allowed"
    fi
  fi

  if has "$PUSH_RE" "$s"; then
    local args="" dir branch
    [[ $s =~ $PUSH_ARGS_RE ]] && args=" ${BASH_REMATCH[1]}"
    if has "(^|[[:space:]])(--force|--force-with-lease(=[^[:space:]]*)?|--force-if-includes|--mirror|--delete|--prune|-[[:alnum:]]*[fd][[:alnum:]]*)${B_END}" "$args" \
       || has "(^|[[:space:]])\\+[^[:space:]]" "$args"; then
      block "force, mirror or delete pushes are not allowed"
    fi
    if has "(^|[[:space:]]|:|refs/heads/)(main|master)${B_END}" "$args"; then
      block "pushing to main/master is not allowed; push the task branch and open a PR"
    fi
    dir=""
    json_get cwd && dir="${JV//\\//}"
    [ -z "$dir" ] && { set_project_dir; dir="$PD"; }
    branch="$(git -C "$dir" symbolic-ref --short -q HEAD 2>/dev/null || git -C "$dir" rev-parse --abbrev-ref HEAD 2>/dev/null || true)"
    if [ "$branch" = "main" ] || [ "$branch" = "master" ]; then
      block "you are on $branch; switch to a task branch before pushing"
    fi
  fi

  # Writes to protected paths. Reads (cat, grep, git diff, ...) stay allowed.
  if has_i "$PROT_CMD" "$s"; then
    if has_i "$REDIRECT_PROT_RE" "$s"; then block "redirecting output into a protected path"; fi
    if has "$WRITE_WORD_RE" "$s"; then block "file commands (mv/cp/rm/touch/chmod/ln/tee/...) on a protected path"; fi
    if has "$INPLACE_RE" "$s"; then block "in-place edit (sed -i / perl -i) of a protected path"; fi
    if has "$GIT_WRITE_RE" "$s"; then block "git checkout/restore/rm/mv/reset on a protected path"; fi
    if has "$INTERP_RE" "$s"; then block "inline interpreter code that mentions a protected path"; fi
  fi
}

bash_guard() {
  local cmd="" agent="" ncmd i
  json_get command && cmd="$JV"
  [ -z "$cmd" ] && cmd="$PAYLOAD"
  json_get agent_type && agent="$JV"
  ncmd="${cmd//\\//}"

  if has_i "--dangerously-skip-permissions|--setting-sources|disableAllHooks" "$ncmd"; then
    block "permission/hook bypass flags are not allowed"
  fi
  if has "(^|[[:space:]])--no-verify${B_END}" "$ncmd"; then
    block "--no-verify skips git hooks"
  fi
  if has_i "api\\.github\\.com" "$ncmd" \
     && has_i "${B_START}(curl|wget|invoke-webrequest|invoke-restmethod|iwr|irm)(\\.exe)?${B_END}" "$ncmd"; then
    block "direct GitHub API calls are not allowed"
  fi
  if [ -n "$agent" ]; then
    if has "$PUSH_RE" "$ncmd" || has "$GH_ANY_RE" "$ncmd"; then
      block "subagent '$agent' may not use git push or gh; only the Producer talks to GitHub"
    fi
  fi
  # cd into a protected folder followed by any write-like command.
  if has_i "${B_START}(cd|pushd)[[:space:]]+[\"${SQ}]?[^;&|]*${PROT_CMD}" "$ncmd" \
     && { has ">" "$ncmd" || has "$WRITE_WORD_RE" "$ncmd" || has "$INPLACE_RE" "$ncmd" \
          || has "$GIT_WRITE_RE" "$ncmd" || has "$INTERP_RE" "$ncmd"; }; then
    block "writing inside a protected folder"
  fi

  split_segments "$ncmd"
  i=0
  while [ $i -lt ${#SEGS[@]} ]; do check_segment "${SEGS[$i]}"; i=$((i + 1)); done

  # Inline interpreter code (python -c "...", node -e "...") that mentions a protected path.
  split_segments "$ncmd" coarse
  i=0
  while [ $i -lt ${#SEGS[@]} ]; do
    if has "$INTERP_RE" "${SEGS[$i]}" && has_i "$PROT_CMD" "${SEGS[$i]}"; then
      block "inline interpreter code that mentions a protected path"
    fi
    i=$((i + 1))
  done
  exit 0
}

path_guard() {
  [ $# -eq 0 ] && block "no allowed prefixes configured for this agent"
  local fp="" rel="" lrel cand p lp lfp
  json_get file_path && fp="$JV"
  [ -z "$fp" ] && json_get notebook_path && fp="$JV"
  [ -z "$fp" ] && block "could not read tool_input.file_path"
  fp="${fp//\\//}"

  if has "^(/|[A-Za-z]:/)" "$fp"; then
    set_project_dir
    local cands="$PD"
    if has "^[A-Za-z]:/" "$PD"; then
      cands="$cands
/$(lower "${PD%%:*}")${PD#?:}"
    elif has "^/[A-Za-z]/" "$PD"; then
      cands="$cands
$(printf '%s' "${PD:1:1}" | tr '[:lower:]' '[:upper:]'):${PD:2}"
    fi
    lfp="$(lower "$fp")"
    while IFS= read -r cand; do
      [ -z "$cand" ] && continue
      case "$lfp" in
        "$(lower "$cand")"/*) rel="${fp:$((${#cand} + 1))}"; break ;;
      esac
    done <<CANDEOF
$cands
CANDEOF
    [ -z "$rel" ] && block "path is outside the project: $fp"
  else
    rel="$fp"
  fi

  while [[ $rel == *//* ]]; do rel="${rel//\/\//\/}"; done
  while [[ $rel == */./* ]]; do rel="${rel//\/.\//\/}"; done
  rel="${rel#./}"
  if has "(^|/)\\.\\.(/|\$)" "$rel"; then block "'..' is not allowed in paths: $rel"; fi
  lrel="$(lower "$rel")"
  if has "$PROT_REL" "$lrel"; then block "protected path: $rel (write work/<n>/governance-proposal.md instead)"; fi

  for p in "$@"; do
    lp="$(lower "${p#./}")"
    case "$lp" in
      */) case "$lrel" in "$lp"*) exit 0 ;; esac ;;
      *)  [ "$lrel" = "$lp" ] && exit 0 ;;
    esac
  done
  block "$rel is outside this agent's write paths ($*)"
}

# The model of the session that runs this hook: the SessionStart "model" field if present; else the
# --model flag of the claude process that started us (the desktop app launches sessions that way and
# omits the field); else $ANTHROPIC_MODEL; else empty (unknown). Read-only, best effort, at most ~5s.
detect_model() {
  json_get model && [ -n "$JV" ] && { printf '%s' "$JV"; return; }
  local m="" pid i args pp
  if [ -r "/proc/$$/winpid" ] && command -v powershell.exe >/dev/null 2>&1; then
    # Git Bash hides Windows parents between its own processes: climb its /proc ppid chain to the
    # topmost Git Bash process first, then follow the Windows parent chain from there.
    pid=$$
    while [ -r "/proc/$pid/ppid" ]; do
      pp="$(cat "/proc/$pid/ppid")"
      { [ "$pp" -le 1 ] 2>/dev/null || [ ! -r "/proc/$pp/winpid" ]; } && break
      pid="$pp"
    done
    m="$(run_timeout 5 powershell.exe -NoProfile -NonInteractive -Command "\$p = Get-CimInstance Win32_Process -Filter \"ProcessId=$(cat "/proc/$pid/winpid")\"; for (\$i = 0; \$i -lt 10 -and \$p; \$i++) { \$x = [regex]::Match([string]\$p.CommandLine, '--model[= ]+([^ \"]+)'); if (\$x.Success) { \$x.Groups[1].Value; break }; \$p = Get-CimInstance Win32_Process -Filter (\"ProcessId=\" + \$p.ParentProcessId) }" 2>/dev/null | tr -d '\r')"
  elif command -v ps >/dev/null 2>&1; then
    pid="$PPID"; i=0
    while [ -n "$pid" ] && [ "$pid" -gt 1 ] 2>/dev/null && [ $i -lt 10 ]; do
      args="$(ps -o args= -p "$pid" 2>/dev/null)"
      if [[ $args =~ --model[=[:space:]]+([^[:space:]]+) ]]; then m="${BASH_REMATCH[1]}"; break; fi
      pid="$(ps -o ppid= -p "$pid" 2>/dev/null | tr -d ' ')"; i=$((i + 1))
    done
  fi
  [ -z "$m" ] && m="${ANTHROPIC_MODEL:-}"
  printf '%s' "$m"
}

# JSON string body (without quotes) for $1.
json_escape() {
  local s
  s="$(printf '%s' "$1" | tr -d '\000-\010\013\014\016-\037')"
  s="${s//\\/\\\\}"; s="${s//\"/\\\"}"; s="${s//$'\t'/\\t}"; s="${s//$'\r'/}"; s="${s//$'\n'/\\n}"
  printf '%s' "$s"
}

session_context() {
  set_project_dir
  cd "$PD" 2>/dev/null || true
  local branch n model warn="" ctx
  # The Producer must run on Sonnet. The desktop app's model picker outranks .claude/settings.json.
  model="$(detect_model)"
  if [ -n "$model" ] && ! has_i "sonnet" "$model"; then
    warn="⚠ MODEL WARNING: this session runs on $model, but the Producer should run on Sonnet. In the desktop app pick Sonnet in the model picker (it can be changed mid-session), or run /model sonnet."
  fi
  ctx="$({
    branch="$(git rev-parse --abbrev-ref HEAD 2>/dev/null || echo '?')"
    echo "Branch: $branch | Last commit: $(git log -1 --format='%h %s' 2>/dev/null | cut -c1-100)"
    case "$branch" in
      task/*)
        n="${branch#task/}"; n="${n%%-*}"
        if [ -f "work/$n/log.md" ]; then echo "work/$n/log.md (last 5 lines):"; tail -n 5 "work/$n/log.md"; fi
        if [ -f "work/$n/plan.md" ]; then
          echo "Unchecked in work/$n/plan.md:"; grep -E '^[[:space:]]*- \[ \]' "work/$n/plan.md" | head -n 6
        fi ;;
      setup/*)
        if [ -f work/setup/progress.md ]; then
          echo "Unchecked in work/setup/progress.md:"; grep -E '^[[:space:]]*- \[ \]' work/setup/progress.md | head -n 8
        fi ;;
    esac
    echo "Open PRs:"
    run_timeout 10 gh pr list --state open --limit 5 2>/dev/null || echo "(gh pr list unavailable)"
  } 2>/dev/null | head -n 23)"
  ctx="$ctx
Run /resume-task to continue, /inbox for client to-dos."
  if [ -z "$warn" ]; then
    printf '%s\n' "$ctx"
  else
    # JSON so the client sees the warning directly (systemMessage) and the Producer gets it as context.
    printf '{"systemMessage":"%s","hookSpecificOutput":{"hookEventName":"SessionStart","additionalContext":"%s"}}\n' \
      "$(json_escape "$warn")" "$(json_escape "$warn Tell the client this first.
$ctx")"
  fi
  exit 0
}

build_gate() {
  local active=false out
  has '"stop_hook_active"[[:space:]]*:[[:space:]]*true' "$PAYLOAD" && active=true
  set_project_dir
  cd "$PD" 2>/dev/null || exit 0
  [ -f Boxing.sln ] || exit 0
  if out="$(dotnet build Boxing.sln -warnaserror -nologo -v q 2>&1)"; then exit 0; fi
  if [ "$active" = true ]; then
    echo "BUILD STILL FAILING — report STATUS: BLOCKED"
    echo "BUILD STILL FAILING — report STATUS: BLOCKED" >&2
    exit 0
  fi
  {
    echo "BUILD FAILED: dotnet build Boxing.sln -warnaserror. Fix it before you finish. Last 40 lines:"
    printf '%s\n' "$out" | tail -n 40
  } >&2
  exit 2
}

case "$MODE" in
  bash-guard)      bash_guard ;;
  path-guard)      path_guard "$@" ;;
  session-context) session_context ;;
  build-gate)      build_gate ;;
  *) echo "studio-hook.sh: unknown mode '$MODE' (bash-guard | path-guard <prefix...> | session-context | build-gate)" >&2; exit 2 ;;
esac
