using FinderStore.Backend.Application.Common.Interfaces;
using FinderStore.Backend.Domain.Common.Interfaces;
using FinderStore.Backend.Application.Constants;
using FinderStore.Backend.Domain.Entities;
using FinderStore.Backend.Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
namespace FinderStore.Backend.Application.Features.Categories.Commands;

public sealed record CategoryCommandData(string Name, string NameAr, string Slug, string Description, string DescriptionAr, string? ImageUrl, int DisplayOrder, Guid? ParentCategoryId, IFormFile? ImageFile = null);

public enum CategoryCommandResult
{
    Success,
    CategoryNotFound,
    SlugAlreadyExists,
}

public sealed record CreateCategoryCommand(CategoryCommandData Data) : IRequest<CreateCategoryResult>;
public sealed record CreateCategoryResult(CategoryCommandResult Result, Guid? CategoryId = null);

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(command => command.Data).NotNull();
        RuleFor(command => command.Data.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Data.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Data.Slug).NotEmpty().MaximumLength(200);
    }
}

public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CreateCategoryResult>
{
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBlobStorageService _blobStorageService;

    public CreateCategoryCommandHandler(
        ICategoryRepository categories,
        IUnitOfWork unitOfWork,
        IBlobStorageService blobStorageService)
        => (_categories, _unitOfWork, _blobStorageService) = (categories, unitOfWork, blobStorageService);

    public async Task<CreateCategoryResult> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (await _categories.AnyAsync(category => category.Slug == request.Data.Slug, cancellationToken))
            return new(CategoryCommandResult.SlugAlreadyExists);
        var data = request.Data;
        var category = Category.Create(data.Name, data.NameAr, data.Slug, data.Description, data.DescriptionAr, data.ImageUrl, data.DisplayOrder, data.ParentCategoryId);

        if (data.ImageFile is not null && data.ImageFile.Length > 0)
        {
            await using var stream = data.ImageFile.OpenReadStream();
            var fileName = $"categories/{category.Id}/{Guid.NewGuid()}{Path.GetExtension(data.ImageFile.FileName)}";
            var imageUrl =await _blobStorageService.UploadAsync(
                stream,
                fileName,
                data.ImageFile.ContentType,
                BlobContainers.Categories,
                cancellationToken);

            category.Update(
                data.Name,
                data.NameAr,
                data.Slug,
                data.Description,
                data.DescriptionAr,
                imageUrl,
                data.DisplayOrder,
                data.ParentCategoryId);
        }

        await _categories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new(CategoryCommandResult.Success, category.Id);
    }
}

public sealed record UpdateCategoryCommand(Guid Id, CategoryCommandData Data) : IRequest<CategoryCommandResult>;

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Data).NotNull();
        RuleFor(command => command.Data.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Data.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Data.Slug).NotEmpty().MaximumLength(200);
    }
}

public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryCommandResult>
{
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(ICategoryRepository categories, IUnitOfWork unitOfWork)
        => (_categories, _unitOfWork) = (categories, unitOfWork);

    public async Task<CategoryCommandResult> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categories.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
            return CategoryCommandResult.CategoryNotFound;
        if (await _categories.AnyAsync(item => item.Slug == request.Data.Slug && item.Id != request.Id, cancellationToken))
            return CategoryCommandResult.SlugAlreadyExists;
        var data = request.Data;
        category.Update(data.Name, data.NameAr, data.Slug, data.Description, data.DescriptionAr, data.ImageUrl, data.DisplayOrder, data.ParentCategoryId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CategoryCommandResult.Success;
    }
}

public sealed record SetCategoryActiveCommand(Guid Id, bool IsActive) : IRequest<bool>;

public sealed class SetCategoryActiveCommandValidator : AbstractValidator<SetCategoryActiveCommand>
{
    public SetCategoryActiveCommandValidator() => RuleFor(command => command.Id).NotEmpty();
}

public sealed class SetCategoryActiveCommandHandler : IRequestHandler<SetCategoryActiveCommand, bool>
{
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _unitOfWork;

    public SetCategoryActiveCommandHandler(ICategoryRepository categories, IUnitOfWork unitOfWork)
        => (_categories, _unitOfWork) = (categories, unitOfWork);

    public async Task<bool> Handle(SetCategoryActiveCommand request, CancellationToken cancellationToken)
    {
        var category = await _categories.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
            return false;
        if (category.IsActive != request.IsActive)
        {
            category.SetActive(request.IsActive);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        return true;
    }
}