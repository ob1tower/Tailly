using CSharpFunctionalExtensions;
using Tailly.SpecialistService.Core.Common;
using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Core.Models.Applications;
using Tailly.SpecialistService.Core.Models.Specialist;

namespace Tailly.SpecialistService.Application.Service.Interfaces
{
    public interface ISpecialistApplicationService
    {
        Task<Result> ApproveAsync(Guid id, string reviewedBy, string? reviewComment = null);
        Task<Result> AssignInterviewAsync(Guid id, string note, DateTime? interviewDate, string reviewedBy);
        Task<Result<(Specialist Specialist, string TemporaryPassword), Error>> AttachSpecialistAccountAsync(Guid applicationId, string reviewedByAdminId);
        Task<Result<SpecialistApplication, Error>> CreateAsync(SpecialistApplication application);
        Task<Result<(List<SpecialistApplication> items, int total), Error>> GetAllAsync(int page, int limit, SpecialistApplicationStatus? status = null);
        Task<Result> RejectAsync(Guid id, string reason, string reviewedBy);
    }
}