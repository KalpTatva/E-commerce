using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Repository.Helpers;

public class Enums
{
    public enum RoleEnum{
        Admin = 1,
        Buyer = 2,
        Seller = 3
    }

    public enum CategoriesEnum
    {
 	    Accessories = 1,
        Computers = 2,
	    Laptops = 3
    }

    public enum RegisterRoleEnum
    {
        Buyer = 2,
        Seller = 3
    }

    public enum DiscountEnum
    {
        Percentage = 1,
        
        [Display(Name = "Fixed Amount")]
        FixedAmount = 2
    }


    public enum OrderStatusEnum
    {
        Pending = 1,
        Shipped = 2,
        Cancelled = 3,
        Delivered = 4
        
    }

    public enum OfferTypeEnum{
        Percentage = 1,

        [Display(Name = "Fixed Price")]
        FixedPrice = 2,

        [Display(Name = "Buy One Get One")]
        BOGO = 3
    }

    public enum ThemeEnum{
        system = 1,
        dark = 2,
        light = 3
    }
}
