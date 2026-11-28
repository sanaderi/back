namespace GamaEdtech.Application.Service
{
    using System.Linq;

    using GamaEdtech.Common.DataAccess.UnitOfWork;
    using GamaEdtech.Domain.Entity.Curriculum;

    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Which subjects topic matching compares, from the curriculum model (phase 8) instead of a list in the code: a GAMA
    /// syllabus with every other board's syllabus of the same subject across boards (<c>CurriculumSyllabus.SubjectId</c>), and
    /// another board's syllabus with GAMA's of that subject. A board added in the admin is matched as soon as its syllabi
    /// have their subject. The ids are the lookup ids the matches carry.
    /// </summary>
    internal static class CurriculumMatching
    {
        internal static async Task<(IReadOnlyList<int> Subjects, bool IsGama)> CandidatesAsync(IUnitOfWork uow, int subjectLegacyId)
        {
            var syllabi = from s in uow.GetRepository<CurriculumSyllabus, int>().GetManyQueryable(t => t.LegacyId != null && t.SubjectId != null)
                          join q in uow.GetRepository<CurriculumQualification, int>().GetManyQueryable() on s.QualificationId equals q.Id
                          join b in uow.GetRepository<CurriculumBoard, int>().GetManyQueryable() on q.BoardId equals b.Id
                          select new { LegacyId = s.LegacyId!.Value, s.SubjectId, b.IsMaster };
            var source = await syllabi.FirstOrDefaultAsync(t => t.LegacyId == subjectLegacyId);
            if (source is null)
            {
                return ([], false);
            }

            var candidates = await syllabi.Where(t => t.SubjectId == source.SubjectId && t.IsMaster != source.IsMaster).OrderBy(t => t.LegacyId).Select(t => t.LegacyId).ToListAsync();
            return (candidates, source.IsMaster);
        }
    }
}
