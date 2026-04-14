using FluentValidation;

namespace E_commerce.v1.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Tên sản phẩm không được rỗng.");
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Giá phải lớn hơn 0.");
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0).WithMessage("Số lượng không được âm.");
        RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Category ID không hợp lệ.");
    }
}
