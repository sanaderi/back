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

## The copy (phase 2)

`ICurriculumCopyService.CopyAsync`, the Hangfire job `CopyCurriculum` (never scheduled; run it from `/hangfire` or with
`POST admin/curriculum/copy`). It reads only this backend's own copies (`LegacyTaxonomyItems`, `LegacyTaxonomyRelations`,
`SchoolBoards`, and the past papers for what they use) and writes only the curriculum tables, so it can run on a live
site. It is **rerunnable**: copied rows are matched by their old id and only updated, never deleted because the source
lost them (old links and content may still point at them). It is refused (`notNative`) once
`WriteRoutes:Curriculum` is `Native`.

| Source | Becomes |
|---|---|
| Boards (list 1) | `CurriculumBoards`; GAMA (7131) the master board, Cambridge (6627) the default with its paper code template, the website's short names (CIE, IB...) |
| Grades (2) | `CurriculumQualifications`, under their board; a grade under several boards is placed under the first and reported |
| Subjects (4) | `CurriculumSyllabus`, under their grade; the code from the metadata or the title ("Mathematics (0580)" → Mathematics, 0580); the subject across boards from `GamaSubjects`' named groups (Biology, Chemistry, Physics, Geology, Mathematics, Further Mathematics, Statistics, Decision Mathematics) |
| Topics (11) | `CurriculumTopics`; gama-api lists them flat, so a unit (`season`) and the chapters after it in list order become a unit with its chapters (a chapter before any unit stays at the top) |
| Classifications (6) that are papers | `CurriculumPapers` of each syllabus whose past papers use them ("Paper 4", chip "4") |
| Classifications that are not papers | `CurriculumResourceTypes` (Worksheets, Predicted Papers...) |
| Variants (29) | Each board's `variant` kind with the values its papers use; `nv` is no variant |
| Extra file types (20) | `CurriculumDocumentTypes` with the website's codes (in, sf, ci, tn, qr, i2, pm), plus qp and ms |
| Links between topics | `CurriculumTopicLinks` (exact) when they join a GAMA topic and another board's; any other link is reported. A link removed at the source is removed here |
| `SchoolBoards` | `SchoolQualifications` (school → board), through the native board's code; mirrored the same way |

Set **once** per board or level (an admin's later edits are kept): the sessions per level the website's search showed
(`SEARCH_MONTHS_BY_LEVEL`, with Cambridge's codes, file letters and title words such as "May June"), else one per month
its papers were sat in; the search panel's filters (Level, Subject, Year, Session, Paper, and Variant where the board has
variants); Cambridge's past paper SEO texts and GAMA's tutorial title.

What can't be placed without guessing goes to the **review list** (`CurriculumCopyIssues`, `GET admin/curriculum/copy/issues`),
replaced on each run. **The check** (`GET admin/curriculum/copy/check`) counts each list against its source (missing
must be 0 before phase 3) and how many past papers, tutorials and exams find their board, level, subject, paper or
resource type, variant and topics in the model, with what left the others out.

## Reading the model (phase 3)

**One switch for both sides:** `Migration:Routes:Curriculum.Website`.

- **Content lists** (`search` past papers, study materials, Exam Hub and tutorials tabs, `search/papers`): on `Native` the
  search marks the query it sends to the native sources (`CurriculumLinking.Flag`, never taken from a visitor), and they
  take their topic links from the model (`CurriculumLinking.PlanAsync`): one hop through GAMA, exact links, the board's
  tab scope, a unit with its chapters. Rows are ranked own → GAMA → each other board, then in the list's order, and
  each item carries `link_source` (`own`, `gama`, `otherBoard`) for the website's labels. Content still holds the lookup
  ids, so the model is joined through each row's `LegacyId`. `Legacy` and `Shadow` keep gama-api's chain: the new links
  differ on purpose, so a comparison would only report the decision. The search's cache key includes the route.
- **Public read API** `api/v1/curriculum/...` (anonymous, cached five minutes). Every answer is
  `{ native, data }`; the website uses `data` only when `native` is true and keeps its own code otherwise, so rollback
  is setting the route back.

| Endpoint | Answers |
|---|---|
| `boards`, `boards/{id}/levels`, `levels/{id}/subjects`, `subjects/{id}/topics` | The pickers' lists, each item with its `legacyId` (what today's search and forms take) |
| `filters?section=&base=&lesson=&isPaper=` | The board's panel in order: key, the search `param` it sets, label, style, options with live counts (empty ones left out) |
| `seo?page=&section=&{values}` | A page's title and description from the board's template, else the default |
| `seo/paper/{id}` | A past paper's or study material's texts with every value worked out from the model (code, session words, paper and variant, file list), as `useCambridgeSeo` wrote them |
| `tree/{subjectId}` | The lesson tree from the model with linked tutorials, in the shape of `search/tutorials/lessonTree` |
| `boards/{id}/schools?cityId=` | Schools teaching the board (`SchoolQualifications`) |
| `landing/{board}/{level?}/{subject?}` | A landing page by slugs: what is under it, paper count, SEO texts, schools |

## The write switch (phase 5)

- **Content keys.** Past papers, tutorials and exams get nullable `CurriculumBoardId`, `CurriculumQualificationId`,
  `CurriculumSyllabusId` (papers also `CurriculumPaperId`, `CurriculumVariantOptionId`, `CurriculumSeriesTypeId`) and
  their topics as rows of `CurriculumContentTopics`. `LinkCurriculumContent` (every 15 minutes) works them out from the
  lookup ids for every row that is new or changed since its `CurriculumLinkDate`, in batches of 1000, each row once per
  run. The lookup ids stay as they are: they are what old links and gama-api's copies use.
- **`Migration:WriteRoutes:Curriculum` = `Native`**: the copy refuses to run, the old lists' editor (`admin/types`) refuses
  changes (`curriculumNative`; the website hides it), and approving a topic match also makes the curriculum's exact link
  (taking it back removes it), so the review keeps working.
- **Sitemap**: `sitemap-boards*.xml` lists the board, qualification and syllabus pages once
  `Routes:Curriculum.Website` is `Native`.
- **`GET api/v1/curriculum/status`**: `{ readNative, writeNative }`, for the website and the admin.

The order of the switch: run the copy and check it (missing 0); `Routes:Curriculum.Website` `Shadow`, then `Native`
(visitors see the model); `WriteRoutes:Curriculum` `Native` (edits move to Admin › Curriculum). The old tables are
dropped in a later, separate change, weeks after.

## Paper detection (phase 6)

When a paper is uploaded, its cover is read with the curriculum model first once `Routes:Curriculum.Website` is
`Native` (`PastPaperDetectService.ReadWithCurriculum`), for any board that has a paper code template:

- **The board:** its name or short name on the page.
- **The printed code:** the board's template up to its session part, with the board's own syllabus codes in place of
  `{syllabusCode}`. `{paperVariant}` is the paper's digit then the variant's (`42`; `0Y` or one digit is that paper with no
  variant), and `{paper}` and `{variant}` are read on their own. For example, Cambridge's `{syllabusCode}/{paperVariant}`
  reads `0625/42`, and a template `{syllabusCode}/{paper}` reads NECTA's `031/2`.
- **The session:** the board's session names or their title words followed by a year.
- **The answer:** in the lookup ids the add form saves (`source` = `curriculum`).

A page no board fits, and the site's predicted papers, go to the Cambridge patterns and the AI as before. So a board added
in the admin with its codes, papers and sessions is detected without a code change.

## After the write switch (phase 7)

What keeps working once `WriteRoutes:Curriculum` is `Native`:

- **New rows get lookup ids.** Every past paper, tutorial, exam, form and list still speaks the lookup lists' ids. So a
  board, level, syllabus, topic, paper or variant made or renamed in Admin › Curriculum is also written to
  `LegacyTaxonomyItems` (`CurriculumMirror`), with the parents and flags gama-api used. Topics are numbered flat in tree
  order with units as seasons; a paper or variant reuses the board's item of that title. The row gets that id as its
  `LegacyId`. A board made in the admin then has papers, exams, tutorials, schools, filters, links and detection like
  any other, and `LinkCurriculumContent` places its content.
- **It waits for the lists' own switch.** The curriculum's writes are refused (`typesNotNative`) until
  `WriteRoutes:Types` is `Native`, so gama-api's copy can never bring an id that is made here.
- **Schools.** Saving a school's boards also brings its `SchoolQualifications` in step (`SyncSchoolAsync`), so they no
  longer wait for the copy.
- **New lookup items before the switch.** The copy runs daily, so items added on gama-api or in `admin/types` reach the
  model until the switch (then it is refused, and nothing new can come from there).

Switch order: `WriteRoutes:Types` → `Native`, then `WriteRoutes:Curriculum` → `Native`. Dropping the old tables is a
separate change, weeks later.

## Topic matching on the model (phase 8)

- **Exact or partial.** A reviewer approves a match as `Exact` (the default; lists bring in each other's content) or
  `Partial` (only "related" on a page): `POST admin/topicmatches/{id}/approve?kind=`. The kind is stored on the match
  (`TopicMatches.Kind`). Only an exact approval also writes gama-api's kind of link (`LegacyTaxonomyRelations`).
- **The curriculum's link follows at once.** Approving creates the `CurriculumTopicLinks` row with the match's kind, and
  taking it back removes it, before the write switch too; the copy never makes a pair twice.
- **The subjects compared come from the model.** Once `WriteRoutes:Curriculum` is `Native`, a GAMA syllabus is compared
  with every other board's syllabus of the same subject across boards (`CurriculumSyllabus.SubjectId`), and back
  (`CurriculumMatching`). A board added in the admin is matched as soon as its syllabi have their subject.
  `GamaSubjects` stays as the fallback for a syllabus with no subject, and goes with the old tables.
- **The review's subject filter** (`subjectId`, either side) now filters; it was ignored before.

## Admin API

`api/v1/admin/curriculum/...`, admins and anyone given the delegated permission (a board edit touches no money or users,
so no authenticator code). Lists of a board's or syllabus's parts (`filters`, `links`, `series-types`, `variant-kinds`,
`papers`) are saved whole: an item with id 0 is added, one left out is removed. Previews: `boards/{id}/panel` (the
filter panel as visitors will see it; years and counts come with the papers in phase 3), `boards/{id}/linked-topics/{topicId}?tab=`,
`seo-preview`.
