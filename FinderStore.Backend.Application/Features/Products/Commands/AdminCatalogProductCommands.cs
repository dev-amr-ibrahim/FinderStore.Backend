using FinderStore.Backend.Domain.Common.Interfaces;
using FinderStore.Backend.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace FinderStore.Backend.Application.Features.Products.Commands;

public sealed record SetProductActiveCommand(Guid Id, bool IsActive, string UpdatedBy) : IRequest<bool>;

public sealed class SetProductActiveCommandValidator : AbstractValidator<SetProductActiveCommand>
{
    public SetProductActiveCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.UpdatedBy).NotEmpty();
    }
}

public sealed class SetProductActiveCommandHandler : IRequestHandler<SetProductActiveCommand, bool>
{
    private readonly IProductRepository _products;
    private readonly IUnitOfWork _unitOfWork;

    public SetProductActiveCommandHandler(IProductRepository products, IUnitOfWork unitOfWork)
        => (_products, _unitOfWork) = (products, unitOfWork);

    public async Task<bool> Handle(SetProductActiveCommand request, CancellationToken cancellationToken)
    {
        var product = await _products.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
            return false;

        if (product.IsActive != request.IsActive)
        {
            product.SetActive(request.IsActive, request.UpdatedBy);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return true;
    }
}

public enum AssignProductCategoryResult
{
    ProductNotFound,
    CategoryNotFound,
    Assigned
}

public sealed record AssignProductCategoryCommand(Guid ProductId, Guid CategoryId, string UpdatedBy)
    : IRequest<AssignProductCategoryResult>;

public sealed class AssignProductCategoryCommandValidator : AbstractValidator<AssignProductCategoryCommand>
{
    public AssignProductCategoryCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.CategoryId).NotEmpty();
        RuleFor(command => command.UpdatedBy).NotEmpty();
    }
}

public sealed class AssignProductCategoryCommandHandler : IRequestHandler<AssignProductCategoryCommand, AssignProductCategoryResult>
{
    private readonly IProductRepository _products;
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _unitOfWork;

    public AssignProductCategoryCommandHandler(IProductRepository products, ICategoryRepository categories, IUnitOfWork unitOfWork)
        => (_products, _categories, _unitOfWork) = (products, categories, unitOfWork);

    public async Task<AssignProductCategoryResult> Handle(AssignProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var product = await _products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return AssignProductCategoryResult.ProductNotFound;

        if (!await _categories.AnyAsync(category => category.Id == request.CategoryId, cancellationToken))
            return AssignProductCategoryResult.CategoryNotFound;

        product.AssignCategory(request.CategoryId, request.UpdatedBy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return AssignProductCategoryResult.Assigned;
    }
}