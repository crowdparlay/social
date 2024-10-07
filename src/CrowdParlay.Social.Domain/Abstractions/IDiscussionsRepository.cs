using CrowdParlay.Social.Domain.DTOs;
using CrowdParlay.Social.Domain.Entities;

namespace CrowdParlay.Social.Domain.Abstractions;

public interface IDiscussionsRepository
{
    public Task<Discussion> GetByIdAsync(Guid id);
    public Task<Page<Discussion>> GetAllAsync(int offset, int count);
    public Task<Page<Discussion>> GetByAuthorAsync(Guid authorId, int offset, int count);
    public Task<Guid> CreateAsync(Guid authorId, string title, string description);
}