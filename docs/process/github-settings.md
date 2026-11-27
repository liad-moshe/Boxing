# GitHub settings (live on GitHub, not in git)

These settings live on github.com, not in the repo. This file records what was configured, the exact
JSON, and where to view or change each setting in the GitHub UI. Configured on 2026-11-27, in setup
Stage 5, with the client's full-power login. Every change was approved by the client first. This file is
protected: agents never edit it.

**Who can change these:** only the client, in the browser, or with a full-power login. The fine-grained
token used on the client's machine after setup has no Administration, Environments, Deployments or
Workflows permission, so agents can't read-modify-write any of this.

If you change a setting in the UI, update this file in a PR, as a governance change.

---

## 1. Labels
**UI:** Issues → Labels (https://github.com/liad-moshe/Boxing/labels)
**Created with:** `gh label create "<name>" --color <hex> --description "<text>" --force`

| Name | Color | Description |
|---|---|---|
| type:mechanic | 1d76db | New or changed combat mechanic (full route) |
| type:ui | 5319e7 | Menu, HUD or other UI change |
| type:bug | d73a4a | Something is broken (repro test first) |
| type:balance | fbca04 | Tuning numbers in data/balance |
| type:refactor | c2e0c6 | Code restructuring, no behaviour change |
| type:visual | f9a8d4 | Procedural visuals or audio (M2+) |
| type:docs | 0075ca | Documentation or planning only |
| route:full | 0052cc | Client forces the full route |
| route:lean | bfdadc | Client forces the lean route |
| needs:boxing | b60205 | Needs boxing-consultant review |
| needs:art | f9d0c4 | Needs tech-artist (M2+) |
| needs:design | d4c5f9 | Needs game-designer spec work |
| status:in-progress | 0e8a16 | A studio task is working on this |
| status:needs-client | e99695 | Waiting for the client's answer |
| status:blocked | 000000 | Blocked after 3 failures; client decides |
| status:ready-for-review | 2cbe4e | PR ready for playtest and approval |
| governance | ff7b00 | Touches protected paths (rules, agents, CI) |

GitHub's 9 default labels (bug, enhancement, …) were kept.

## 2. Merge settings
**UI:** Settings → General → section **Pull Requests**
```
PATCH /repos/liad-moshe/Boxing
{ "allow_squash_merge": true, "allow_merge_commit": false, "allow_rebase_merge": false,
  "delete_branch_on_merge": true, "allow_auto_merge": false }
```
Effect:
- Squash is the only merge method.
- Task branches are deleted after merge.
- Nothing merges automatically.

## 3. Actions settings
**UI:** Settings → Actions → General
- **Workflow permissions:** "Read repository contents and packages permissions". "Allow GitHub Actions
  to create and approve pull requests" is **unchecked**.
  ```
  PUT /repos/liad-moshe/Boxing/actions/permissions/workflow
  { "default_workflow_permissions": "read", "can_approve_pull_request_reviews": false }
  ```
- **Approval for running fork pull request workflows from contributors:** "Require approval for all
  external contributors".
  ```
  PUT /repos/liad-moshe/Boxing/actions/permissions/fork-pr-contributor-approval
  { "approval_policy": "all_external_contributors" }
  ```
Effect:
- Workflow tokens are read-only unless a job asks for more. Only `governance` (PR comment) and
  `release` (create a release) ask.
- A stranger's fork PR runs no workflow until the client clicks **Approve and run**.

## 4. Environment `client-approval` (the G3 gate)
**UI:** Settings → Environments → **client-approval**
```
PUT /repos/liad-moshe/Boxing/environments/client-approval
{ "wait_timer": 0, "prevent_self_review": false,
  "reviewers": [ { "type": "User", "id": 28000690 } ],
  "deployment_branch_policy": null }
```
Set by the client in the UI, because the field is not in GitHub's REST docs: **"Allow administrators to
bypass configured protection rules" = unchecked**. The API then reports `"can_admins_bypass": false`.

- **Required reviewer:** liad-moshe, user id 28000690 (read from `gh api user`).
- **Prevent self-review: OFF, on purpose.**
  - GitHub treats whoever triggered the run as "self", and every run is triggered by a push made with
    the client's own account.
  - With self-review prevented, no one could ever approve.
  - Agents still can't approve: the machine's token has no Deployments/Environments permission, and the
    bash guard blocks `gh api`.
- **Admin bypass: OFF.** Even the repo admin must approve the deployment.
- **Deployment branches:** any branch, since all task branches need the gate.
- The `client-approval` job in ci.yml has `permissions: {}`, no checkout, and runs no repo code.

## 5. GitHub Actions app id
Read from the check runs on PR #1's head with
`gh api repos/liad-moshe/Boxing/commits/<sha>/check-runs --jq '.check_runs[] | {name, app_id: .app.id}'`
→ **15368** (`github-actions`). It is used as `integration_id` below, so only checks produced by GitHub
Actions count. A status posted by anything else does not.

## 6. Ruleset `protect-main` (id 28200762)
**UI:** Settings → Rules → Rulesets → **protect-main**
```json
{
  "name": "protect-main",
  "target": "branch",
  "enforcement": "active",
  "bypass_actors": [],
  "conditions": { "ref_name": { "include": ["~DEFAULT_BRANCH"], "exclude": [] } },
  "rules": [
    { "type": "deletion" },
    { "type": "non_fast_forward" },
    { "type": "pull_request",
      "parameters": {
        "required_approving_review_count": 0,
        "dismiss_stale_reviews_on_push": false,
        "require_code_owner_review": false,
        "require_last_push_approval": false,
        "required_review_thread_resolution": false,
        "allowed_merge_methods": ["squash"]
      } },
    { "type": "required_status_checks",
      "parameters": {
        "strict_required_status_checks_policy": true,
        "do_not_enforce_on_create": false,
        "required_status_checks": [
          { "context": "build-test",      "integration_id": 15368 },
          { "context": "kb-check",        "integration_id": 15368 },
          { "context": "client-approval", "integration_id": 15368 }
        ]
      } }
  ]
}
```
GitHub also adds `require_extra_approval_for_unattributed_changes: true` by default. It only affects
PRs opened by GitHub Copilot, and only when approvals are required (ours: 0), so it has no effect here.

Effect:
- No bypass for anyone, admins included.
- `main` can't be deleted or force-pushed.
- Every change comes through a squash-merged PR whose build-test, kb-check and client-approval checks
  passed.
- The branch must be up to date with main (strict). If GitHub shows **Update branch**, CI re-runs and the
  client approves again.
- `governance` is deliberately not required: it only comments.

## Verified (2026-11-27)
On PR #1, run 45912170910:
- `client-approval` waits for review (reviewer liad-moshe).
- build-test, kb-check and governance passed.
- Merge state: **BLOCKED** until the client approves.

## Read-only commands to inspect the settings (client)
```bash
gh api repos/liad-moshe/Boxing --jq '{allow_squash_merge, allow_merge_commit, allow_rebase_merge, delete_branch_on_merge, allow_auto_merge}'
gh api repos/liad-moshe/Boxing/actions/permissions/workflow
gh api repos/liad-moshe/Boxing/actions/permissions/fork-pr-contributor-approval
gh api repos/liad-moshe/Boxing/environments/client-approval
gh api repos/liad-moshe/Boxing/rulesets/28200762
```
These need a login allowed to read admin settings, which is the full-power login, not the agents' token.
