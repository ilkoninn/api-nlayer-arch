namespace App.Business.Services.Internal.Marketing.Interfaces;

public interface ITeamMemberService
{
    //  ===============================
    //  Public operations
    //  ===============================

    Task<PagedResult<TeamMemberDto>> GetAllAsync(TeamMemberListQueryDto query, CancellationToken ct = default);


    //  ===============================
    //  Admin operations
    //  ===============================

    Task<bool> CreateAsync(CreateTeamMemberDto createDto, CancellationToken ct = default);
    Task<bool> UpdateAsync(Guid id, UpdateTeamMemberDto updateDto, CancellationToken ct = default);
    Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);
}


