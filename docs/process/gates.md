# Your gates (G1–G4)

You own four decisions. Agents can't pass them for you. G3 and G4 are enforced by GitHub, because
your machine's token has no permission to approve deployments or bypass the branch rules.
This file is protected: agents never edit it.

| Gate | When | Where | What you decide |
|---|---|---|---|
| **G1** Route OK | After /task, only if the route includes designer/tech-artist, or is ambiguous | In the Claude chat | Is the classification and the list of roles right? |
| **G2** Spec / look OK | After the designer (or tech-artist) has written | In the Claude chat. Read docs/specs/<feature>.md on the task branch | Is this what you want built? |
| **G3** Playtest + approve | CI is green on the PR | Browser, on github.com | Does the build play right? |
| **G4** Merge | After G3 | Browser, on the PR page | Ship it to main |

## G1 and G2 — in the chat
The Producer asks a direct question and stops. Answer with "yes", or with changes. Nothing after the gate
starts until you answer.

## G3 — playtest, then approve the deployment
1. Open the PR (link in the Producer's message, or the repo's **Pull requests** tab).
2. Read the PR body. It is your CLIENT BRIEFING: what changed, keys, a 5-item playtest checklist.
3. Download the build: on the PR, open **Checks**, choose the build-test workflow run, scroll to
   **Artifacts** and download the playtest zip. Unzip and run it.
4. Go through the checklist.
5. Approve the run. On the same workflow run page a yellow banner says the run is waiting for review:
   - click **Review deployments**,
   - tick **client-approval**,
   - optionally write a comment,
   - click **Approve and deploy**.
   If the build is not OK, click **Reject** instead and tell the Producer what was wrong.

GitHub also sends you a notification when a deployment waits for your review (SETUP.md explains how
to enable it).

## G4 — merge
1. On the PR page, wait until all required checks are green: build-test, kb-check, client-approval.
2. If GitHub shows **Update branch**, click it. The rules require the branch to be up to date with main.
   CI then runs again, and you approve client-approval again (step 5 above).
3. Click **Squash and merge**, then **Confirm squash and merge**.
4. release.yml publishes a `playtest-N` prerelease on the repo's **Releases** page.

## Why the agents can't do this
- The ruleset `protect-main` has no bypass actors: nobody, admins included, can push to main directly.
- The PAT on this machine has no Deployments, Environments, Administration or Workflows permission.
- Agents are told never to merge, never to approve, and never to use a browser.
