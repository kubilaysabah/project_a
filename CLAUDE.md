# CLAUDE.md — project context for Claude sessions

This file makes any Claude session on any machine fully up to speed. Read it as binding project instructions.

## What this project is

**Working title: "project_a" — THE FINAL NAME IS NOT DECIDED YET (see "Name decision" below).**

A first-person **anomaly-loop horror** game ("The Exit 8" formula) set in a 1994 Turkish apartment building ("Huzur Apartmanı"). The player is the building's night janitor (kapıcı): climb floor by floor collecting trash bags; every landing is identical; if the landing is NORMAL continue up, if there is an ANOMALY turn back; a wrong call resets you to floor 1; reach floor 8 without a mistake → rooftop → morning → ending twist (the building is seen from outside to have NINE floors).

- **Goal:** ship on Steam in 2-3 months at $4.99. This is Şahan's quick-ship title (GAME #2 in the Moguls two-developer split — Moguls' CLAUDE.md explicitly does NOT track this project; this repo is its own board and memory).
- **Full concept document:** the user's `KAPICI-oyun-fikri.txt` (kept outside the repo); its content is summarized in this file and in the GitHub issues.
- **Engine:** Unity 6.5 (6000.5.5f1), URP (URP template with PC/Mobile render pipeline assets), new Input System. Project lives at the repo root. Each developer's local clone path, hardware and personal preferences live in that developer's own **`CLAUDE.local.md`** (gitignored, auto-loaded by Claude Code) — never in this file.

## Two developers, one shared memory

- **Şahan** (bozsahan@gmail.com) — the developer being taught; owns the game, does all Editor-side work. **Kubilay** (`kubilaysabah`) — repo owner, Moguls lead, reviews and co-decides (name, budget, Steam).
- Shared, project-wide facts and rules → this `CLAUDE.md` (committed). Machine- or person-specific facts (paths, GPU, installed tools, local AI, personal habits) → `CLAUDE.local.md` (gitignored; each dev keeps their own). Claude's per-machine memory directory is the third layer and is never committed.
- When a rule here changes, the change is committed as `docs:` so the other developer's next session picks it up.

## Name decision (OPEN — one of the first work items, issue #1)

Steam already has a game called **KAPICI** (Antumbra Games, app 3344550, Q4 2026, single-night narrative horror in a Turkish apartment with a janitor). Same space, same word, overlapping release window — the name KAPICI is unusable and launching as "the second janitor game" must be avoided. **User decision 2026-09-20: the FORMULA stays (anomaly loop) and the APARTMENT skin stays; only the NAME changes.** The concept file's "SON DOLMUŞ" reskin (night minibus, empty Steam niche, even smaller scope) is the documented FALLBACK if apartment-horror positioning vs KAPICI proves bad — do not delete that idea.

Steam collision scan (2026-09-20): "Last Floor" EXISTS (anomaly horror — kills "Son Kat" in translation); "The Stairwell", "Staircase55", "Abnormal Stairs" exist (kills stairwell names); "No Elevator" exists; "Huzur" / "Huzur Apartmanı" is CLEAN; "8. Kat" exact is clean (nearest: "18th Floor", "The 18th Floor", holo8). Candidates live in issue #1; decision is made together with the user, then this heading is replaced by the real name everywhere.

## This is a teaching project — work in mentor mode

The developer on this project is **Şahan** — the second developer from Moguls, taught in the same mentor setup. The rules are binding:

- **Converse in Turkish.** Commit messages and issue titles are English; issue bodies and chat are Turkish.
- **Code comments are in Turkish, line-by-line, teaching style** — every line explained (what it does and why) in the code itself. Chat is for concepts; the code explains itself.
- C# level: basics only. Explain any feature beyond variables/loops/arrays/classes/conditions/functions (generics, events, delegates, interfaces, enums, LINQ, ScriptableObject API) the first time it is used.
- Unity level: beginner. Explain every new Unity concept (the *why*, not just the *how*) the first time it appears.
- **No frontend/web analogies.** Use universal real-world analogies. Explanations slow, detailed, in proper natural Turkish; one idea at a time; define every term before using it.
- **Editor-side steps are done BY the user** with click-by-click instructions. Code is written together, in small commented pieces.
- Do NOT quiz the user; explain answers directly.
- **COMPILE CHECK IS NON-NEGOTIABLE:** after every code step, the Unity Console must show 0 errors before moving on. Never assume a compile succeeded.

## Git conventions (strict)

- Commit messages 100% English, Conventional Commits format.
- **NEVER add any AI signature, Co-Authored-By trailer, or "Generated with" attribution to commits or PRs.** Explicit standing instruction.
- Commit at the end of each lesson once verified working. Remind the user to save the scene (Ctrl+S) first — unsaved Unity scene changes are invisible to git.
- Settings-file churn (`ProjectSettings/` alone) goes in separate `chore` commits.
- `.gitattributes` uses Git LFS macros for binaries (from the standard Unity template) — verify `git lfs install` has been run locally before the first binary asset lands. **Known gap:** the initial check-in happened before LFS was active, so `Assets/TutorialInfo/Icons/URP.png` and `Layout.wlt` are raw blobs in history and show as "modified" on an LFS-enabled clone; fix = `git add --renormalize .` + `chore` commit (tracked as an issue), no history rewrite without both devs agreeing.
- `CLAUDE.local.md` is gitignored (personal per-developer instructions). `.mcp.json` IS committed (shared Unity MCP config, path-independent).

## GitHub workflow (binding — all work lives on GitHub)

- Repo: `kubilaysabah/project_a`. Task board: **GitHub Projects panel https://github.com/users/kubilaysabah/projects/4** + milestones for the 12-week plan.
- Every work item is a GitHub issue: **English imperative title, Turkish body with Amaç / Kapsam / Kabul kriterleri**, attached to a milestone and the Projects board.
- While working an issue: move its board Status (Todo → In progress → Done), **assign the issue to the developer doing the work** (`gh issue edit N --add-assignee @me`) the moment it goes to In progress, and leave **detailed** progress comments (what changed and why, commit hashes, what was verified, what remains, any decision taken). One comment per meaningful step, not one per session.
- Closing an issue requires user verification first; close with a final comment naming the verifying test.
- New work that surfaces in conversation is opened as an issue in the same turn it is agreed.
- All of this is done live from the terminal with the **GitHub CLI (`gh`)** — issues, comments, assignment, milestones, board status (`gh project item-edit`). Milestones and board items CAN be managed with a locally authenticated `gh`; only remote/MCP-only sessions lack that ability.

## Tooling: gh CLI + Unity CLI + Claude (the working desk)

- **GitHub CLI (`gh`)** — every issue/board/PR action. Must be authenticated per machine by the developer (`gh auth login`); Claude never enters credentials.
- **Unity CLI (`unity`, experimental)** — installs/opens editors and, through the project's `com.unity.pipeline` package (in `Packages/manifest.json`), drives a *running* Editor from the terminal: `unity status` (is an Editor connected?), `unity command` (list Editor-exposed commands), `unity command eval '<C#>'`, `unity test`, `unity build`. The Editor must be open with this project for `command` to work; each dev signs in with `unity auth login`.
- **Claude ↔ Unity:** the Unity CLI agent skill is installed per machine (`unity skill install claude-code` → `~/.claude/skills/unity-cli`), and the repo ships a project-level `.mcp.json` that starts `unity mcp` as an MCP server so Claude can inspect/modify the open scene directly. Claude may use these to inspect the hierarchy, check the Console for compile errors, or run tests — but **Editor-side lesson steps are still explained click-by-click and done by the user** (mentor rule); CLI automation is for verification and repetitive chores, not for skipping the teaching.
- **Compile check via CLI:** after every code step, in addition to looking at the Console, Claude may run `unity command` to query the Editor for compile status; the rule (0 errors before moving on) is unchanged.
- **Local AI (optional, per machine):** cheap, repetitive or bulk tasks (TR/EN string drafts, anomaly brainstorm lists, texture/decal drafts, log summaries) go to local models (Ollama / ComfyUI etc.) rather than Claude tokens. What is installed where is recorded in each dev's `CLAUDE.local.md`.

## Architecture decisions (agreed, mirrors Moguls principles)

- **Game logic in plain C# classes**, separate from MonoBehaviours, for testability.
- **CONTENT IS DATA:** every anomaly is a ScriptableObject card (`AnomalyCard`: id, tier, difficulty, what it changes on the landing). The landing is built/reset from data; adding anomaly #47 must require ZERO new code, only a new asset. This is the whole game — one landing + stair loop modeled once, 60+ data-driven variations.
- Single modeled space: one landing + one stair flight, looped (player repositioned seamlessly). No open world, no walking nav beyond the loop.
- Anomaly tiers from the concept: **FISILTI** (subtle: reversed door number, changed doormat, single slipper), **TEKİNSİZ** (green bulb, door ajar, moving trash bag, wrong shadow), **KÂBUS** (rare scripted clips: figure at corridor end, all doors same number, bell rings by itself).
- **Sound is the cheapest, strongest horror tool:** normal ambience texture (distant TV, baby crying, pipe knocks) that CORRUPTS on anomaly floors (crying plays reversed, murmur goes silent). Ears notice before eyes.
- Wrong-call penalty is DELAYED, not instant: half a flight up, lights die one by one — that silence is the scariest moment. Design for it.
- Budget principle: free tools + total asset spend ≤ $100 (apartment/retro interior pack $15-30, Turkish identity via textures + ~10 small Blender props: tüp, terlik, ayakkabılık).

## Roadmap — 12 weeks (milestones on GitHub)

- **H1-2 — Graybox:** landing+stair loop, FP controller, floor counter + decision rule, anomaly system (SO cards + spawner), first 10 anomalies, trash-bag task, 60-idea anomaly brainstorm. Code done in 2-3 weeks; the rest of the schedule is content and polish.
- **H3-4 — Real look + STEAM PAGE:** asset purchase (≤$100 list approved first), art pass, URP lighting/post. **Steam page LIVE in week 4 — hard deadline, non-negotiable (wishlists start early).**
- **H5-7 — Content:** anomalies 11-60, sound design, TR+EN strings.
- **H8-9 — Demo:** demo build → keys to 50+ streamers, polish from feedback.
- **H10-12 — Launch:** trailer from in-game capture, release, patch from first reviews.

Honest revenue band: $500-$5,000; the real prize is living the full Steam cycle (page, capsule, demo, launch) once before Moguls ships.

## Living docs (Claude Docs — shared, editable, commentable)

Two living documents mirror this file for humans; keep BOTH in sync with this file when status or decisions change:

- **Proje Hafızası** (session journal + decisions): https://claude.ai/code/artifact/00a0d343-6a47-48dc-807e-b671a03224c1
- **Oyun Tasarım Belgesi** (concept, mechanics, roadmap, workflow): https://claude.ai/code/artifact/78f0fa6f-6087-497b-8492-4576f63bd6ee

This repo's CLAUDE.md stays the canonical, binding session memory (it is what sessions auto-load); the docs are the readable/shareable layer on top.

## Current status (UPDATE THIS SECTION whenever progress is made)

- 2026-09-20: Repo created (name pending, hence `project_a`). Unity 6.5.5f1 URP project generated and pushed ("Initial check-in") with standard Unity .gitignore/.gitattributes (LFS macros). Projects board #4 created by the user. CLAUDE.md written. **The 12-week plan is on GitHub as issues #1-#20** (#1-#8 graybox H1-2, #9-#12 art+Steam page H3-4, #13-#16 content H5-7, #17-#18 demo H8-9, #19-#20 launch H10-12); milestones + board attachment done by the user via a local `gh` script (MCP tooling in remote sessions cannot create milestones/board items — leave that step to the user each time it recurs). Next: name decision (issue #1) + Lesson 1 graybox (landing + stair loop + decision mechanic skeleton, issues #2-#4).
- 2026-09-21 (Şahan's machine, first session): toolchain set up — GitHub CLI 2.101 installed (auth pending, user-side), Unity CLI 1.0.0-beta.8 signed in, project registered in Hub, `com.unity.pipeline 0.7.0-exp.1` added to the manifest, Unity CLI Claude skill installed, project `.mcp.json` added, `CLAUDE.local.md` introduced (gitignored) for per-developer info, Ollama installed for local LLM chores (no model pulled yet). Found the LFS gap (URP.png raw in history) — to be fixed as an issue. Sections added to this file: "Two developers, one shared memory", "Tooling", gh self-assignment rule. Next: `gh auth login` → open LFS-fix + tooling issues → name decision (#1) → Lesson 1.
