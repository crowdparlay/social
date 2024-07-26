using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Domain.DTOs;

namespace CrowdParlay.Social.Application.Abstractions;

public interface IDiscussionsService
{
    public Task<DiscussionDto> GetByIdAsync(Guid id);
    public Task<Page<DiscussionDto>> GetAllAsync(int offset, int count);
    public Task<Page<DiscussionDto>> GetByAuthorAsync(Guid authorId, int offset, int count);
    public Task<DiscussionDto> CreateAsync(Guid authorId, string title, string description);
}