---
name: boxing-sources
description: Rules for any claim about real boxing in this project - allowed source tiers, the [S#] citation format, the docs/boxing/sources.md registry, and how to mark GAMEPLAY ABSTRACTION. Use whenever a spec, review or note says something about real boxing rules, technique, scoring or physiology.
user-invocable: false
---

# Boxing sources and citations (environment-spec.md §7)

## Allowed sources
| Tier | What | Required in the registry |
|---|---|---|
| 1 | Official rulebooks: World Boxing technical and competition rules, Association of Boxing Commissions (ABC) unified rules, national boxing commissions and boards | The **current edition**, checked on the publisher's own site, with the edition or year recorded |
| 2 | Peer-reviewed research | A **DOI** (https://doi.org/...) or a **PubMed** link |
| 3 | Coaching material from credentialed coaches, or national-federation coach-education programmes | The **credential stated**, e.g. "national federation Level 3 coach" |

**NOT sources:** forums, Reddit, unsourced videos, blogs, wikis, news commentary, and AI-generated
summaries, including your own memory. A good-looking secondary page can lead you to a primary source,
but you cite the primary one.

## Registry: docs/boxing/sources.md
One row per source:
`| S<next> | <tier> | <title> | <author/body> | <edition/year> | <URL or DOI> | <YYYY-MM-DD accessed> | <notes> |`
- IDs are permanent. Never renumber or reuse them.
- When a newer edition comes out, add a new row and note "supersedes S<k>". Old citations stay valid.
- Only boxing-consultant adds rows. Other roles cite existing rows, or ask for a new one in QUESTIONS.

## Citing in text
- Put `[S#]` right after the claim, as in "Amateur bouts are scored by judges each round [S2]." If you
  have several sources, write `[S2][S5]`.
- Every factual claim about real boxing needs a citation. Opinions and game design choices don't.
- **Summarize, never copy rulebook text.** Paraphrase in your own words. At most one very short quote
  per note, in quotation marks, only when the exact wording matters.
- A claim you can't source is listed under QUESTIONS, not stated as fact.

## GAMEPLAY ABSTRACTION
Wherever the game deliberately differs from real boxing, write one line:
`GAMEPLAY ABSTRACTION: <what the game does> instead of <what real boxing does [S#]> — <reason (readability, short sessions, controls)>.`
Abstractions are fine and expected. Hiding them is not.

## Review output format (work/<n>/boxing-review.md)
| # | Spec item | Verdict | Note | Sources |
|---|---|---|---|---|
| 1 | AC2 jab range | OK / CHANGE / ABSTRACTION | <one line> | [S1] |
