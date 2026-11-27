# Failure playbook

What to do when something goes wrong. Each entry: **symptom → why it is safe → what to do**.
This file is protected: agents never edit it.

## Usage limit reached mid-task
- **Why it's safe:** the checkpoint protocol commits and pushes after every role step (workflow.md), so
  at most one step is lost.
- **Do:** wait for the limit to reset. Reopen the same conversation (the built-in `/resume`, or pick it
  in the sidebar), then run **`/resume-task`**. The Producer re-reads work/<n>/brief.md, plan.md and log.md
  and picks up from the first unticked item.

## Claude crashes or the machine restarts
- **Why it's safe:** the same checkpoints apply. A SessionStart hook prints the current branch, the last
  commit, and the task's log/plan status into the new session's context.
- **Do:** start `claude` in the repo, a new session or the old one via the built-in `/resume`, and run
  **`/resume-task`**.

> Two different commands: the built-in `/resume` reopens a past *conversation*. The studio skill
> `/resume-task` continues a *task* from the files on its branch, and works even in a brand-new session.

## An agent is stuck or looping
- **Why it's limited:** each agent file sets `maxTurns`, so a runaway agent stops by itself.
- **Do:** nothing at first. The Producer applies the escalation ladder:
  - 2 identical failures → developer re-run on opus.
  - 3 failures → label `status:blocked`, ask you.
  - If you are asked, you can choose: simplify the task, split it, or drop it.

## Context bloat (answers get slow, vague or forgetful)
- **Why it happens:** one session has carried too much history.
- **Do:** one task per session. After `/ship`, run `/clear` before the next task. Long sessions are
  compacted automatically; CLAUDE.md "# Compact instructions" lists what must survive.

## CI fails repeatedly on the PR
- **Do:** tell the Producer "CI failed on PR <n>". It reads the log (`gh run view --log-failed`), counts it as
  a failed cycle and routes back to the developer. After the 3rd failure it marks the task blocked and asks you.

## CI is flaky (fails, then passes with no code change)
- **Do:** on the PR's Checks tab, or the workflow run page, click **Re-run failed jobs**. Agents have no
  Actions-write permission, so only you can re-run. If it happens twice, ask the Producer to open a bug task.

## An agent invents an engine API
- **Why it's caught:** the compiler is the oracle. TreatWarningsAsErrors turns obsolete APIs (CS0618/CS0612)
  into build errors, and the developer must check unfamiliar APIs against the exact-version GodotSharp.xml
  docs or docs.godotengine.org/en/4.7. CI also greps for Godot 3 names.
- **Do:** nothing. The failed build goes back to the developer as a failed cycle.

## Knowledge base is out of date
- **Why it's caught:** CI kb-check fails if a backticked path in docs/kb does not exist, or if an
  InputMap action in project.godot is missing from docs/kb/input-map.md.
- **Do:** nothing. The Producer re-runs kb-keeper on the task branch.

## The PAT expired (gh or git push says 401 / Bad credentials)
- **Do:** create a new fine-grained PAT with exactly the same settings (SETUP.md, "Logins"), then
  `gh auth login --with-token` in your own terminal. GitHub emails you before a token expires.
