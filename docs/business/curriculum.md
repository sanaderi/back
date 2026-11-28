# Curriculum model (any exam board, defined as data)

Since 2026-11-28 (phase 1 of the curriculum proposal). Boards, levels, subjects, papers, sessions and variants are today a
copy of gama-api's lookup lists (`LegacyTaxonomyItems`) plus board details written into the website (Cambridge ids,
sessions per grade, variants 1-3, years, code regexes, Cambridge-only SEO). The curriculum model replaces both with data
an admin edits: define a board, and its filter panel, paper codes, topic links and SEO texts follow.

## Phases and the switch (no downtime)

Each phase is its own PR. Nothing visitors see changes before phase 4, and every step can be undone with a setting.

| Phase | What | Visitors |
|---|---|---|
| P1 (this) | Tables, the admin API (`admin/curriculum`), the link rules, the SEO templates | no change |
| P2 | A rerunnable copy job fills the tables from the lookup lists (each row keeps its old id in `LegacyId`), schools are linked, counts are checked | no change |
| P3 | Filters and search answer from the model; `Migration:Routes:Curriculum` goes `Shadow` (old answer served, new one compared) | no change |
| P4 | `Routes:Curriculum` → `Native`: the website reads the model; board, qualification and syllabus pages | changes |
| P5 | `WriteRoutes:Curriculum` → `Native`: edits are made here only; the old lists are dropped weeks later | — |
| P6 | Paper detection as data, new boards added by admins | — |

`MigrationDomain.Curriculum` uses the same router as the move off gama-api (`docs/architecture/legacy-migration.md`):
the Legacy side is the copied lookup lists, the Native side the `Curriculum*` tables. The tracked `appsettings.json`
keeps both routes `Legacy`; rollback is setting them back. The migration only creates new tables (and seeds the
default SEO templates), so it is safe to apply on a running site.

**Copied rows** (`LegacyId` set) are refused by the admin (`notNative`) until `WriteRoutes:Curriculum` is `Native`,
because the copy job would overwrite the edit. Rows made here, and the board profile (filters, tab scopes, other-board
order, series types, variant kinds, SEO templates), which is never copied, can always be edited, so a board can be set
up before the switch. A copied row is never deleted: old links (`?board=6627`) and content point at it.

## The model

| Table | Holds |
|---|---|
| `CurriculumBoards` | A board: slug, names, country (ISO 3166-1 alpha-2, null = international), parent board, logo, website, `IsMaster` (GAMA, at most one), `IsDefault` (at most one), `IsEnabled` (hidden until on), the paper `CodeTemplate`, default language |
| `CurriculumBoardFilters` | The filters its panel shows (`CurriculumFilterKeys`: level, subject, year, series, paper, tier, `variant.{kind}`, language), each with the board's label ("Exam" for NSC's series), chips or dropdown, order, on/off |
| `CurriculumBoardTabs` | A tab's link scope when it differs from the default (see "Topic links") |
| `CurriculumBoardLinkOrders` | The order of the other boards' items in its lists |
| `CurriculumQualifications` | IGCSE, IB Diploma, CSEE (Form 4), Class 12... (the copied grades) |
| `CurriculumSubjects` | A subject across boards ("Mathematics") |
| `CurriculumSyllabus` | A qualification's subject with the board's code: 0580 Mathematics, 031 Physics (the copied subjects) |
| `CurriculumPapers` | A syllabus's components: "Paper 4" (chip "4"), tier |
| `CurriculumSeriesTypes` | When exams are sat: May/June (code M/J, file letter s, months 5,6), per board or one qualification |
| `CurriculumVariantKinds` / `CurriculumVariantOptions` | Zone 1/2/3, time zone TZ1/TZ2, set 65/2/3... A board with none shows no variant filter |
| `CurriculumDocumentTypes` | qp, ms, insert, audio... with label and icon |
| `CurriculumTopics` | A syllabus's units and chapters, in order (two levels) |
| `CurriculumTopicLinks` | A reviewed link between a GAMA topic and another board's topic, `Exact` or `Partial` |
| `CurriculumSeoTemplates` | Title and description templates per page kind, default (no board) or a board's own |

Names in other languages use the existing `ContentLocalizations` (`admin/contentlocalizations`), with the entity name as
content type (`CurriculumBoard`, `CurriculumSyllabus`...) and the `Languages` table.

## Topic links

Decided 2026-11-28 (`CurriculumLinkRules`):

- Links go **through GAMA only, one hop**: Cambridge reaches Edexcel through the GAMA topic both are linked to, never
  further (today's links chain without limit). A link joins a GAMA topic and another board's topic; the admin refuses
  anything else.
- Only **exact** links bring content into lists. **Partial** ones are only "related" on a detail page.
- Each tab of a board has a **scope**: `Own`, `OwnAndMaster` (own, then GAMA's) or `All` (own, GAMA's, then the other
  linked boards'). Default: **past papers `Own`**, study materials, exams and tutorials `All`. An admin changes it per
  board and tab; a scope set back to its default is not stored.
- The **order** is always own, GAMA, then the other boards: first those in the board's `CurriculumBoardLinkOrders`, then
  the rest by their list order.
- Each group carries its `source` (`own`, `gama`, `otherBoard`), so the website shows "From GAMA" and "Same topic in
  other boards" without knowing any board. A unit's list includes its chapters. The lesson tree follows the tutorials tab.

## SEO titles and descriptions

Written by the backend (`CurriculumTemplate`, `ICurriculumService.GetSeoAsync`), so the website, the sitemaps and the
MCP say the same thing and an admin changes them without a deploy. The migration seeds the defaults (`CurriculumSeoDefaults`):
the website's own texts as of 2026-11-28 (search tabs, paper, study material, exam and tutorial pages), plus new ones
for the board, qualification and syllabus landing pages. A board's own template wins over the default. Phase 2 attaches
Cambridge's paper texts (`CambridgePastPaper`, code `{syllabusCode}/{paperVariant}/{seriesCode}/{yy}`) and GAMA's
tutorial title to those boards.

Template syntax:

- `{name}` is a value; outside brackets it is required.
- `[...]` is optional, dropped when a value in it is empty.
- `a || b` are alternatives: the first whose required values are all there is used; the last can be plain text (the
  fallback when no board is chosen).
- Spaces are collapsed, a space before `. , ; :` is removed, and a phrase of up to four words repeated back to back is
  said once ("Cambridge Cambridge IGCSE" → "Cambridge IGCSE").
- Only the exam page's description is cut (155 characters at a word, as the website did).
- The admin's preview (`POST admin/curriculum/seo-preview`) warns when a title is over 60 characters or a description
  outside 70-160, what search results show.

## Admin API

`api/v1/admin/curriculum/...`, admins and anyone given the delegated permission (a board edit touches no money or users,
so no authenticator code). Lists of a board's or syllabus's parts (`filters`, `links`, `series-types`, `variant-kinds`,
`papers`) are saved whole: an item with id 0 is added, one left out is removed. Previews: `boards/{id}/panel` (the
filter panel as visitors will see it; years and counts come with the papers in phase 3), `boards/{id}/linked-topics/{topicId}?tab=`,
`seo-preview`.
