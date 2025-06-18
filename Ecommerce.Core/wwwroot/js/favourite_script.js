$(".loader3").hide();

$(document).ready(function () {
    // function for getting product on page
    function FetchProducts() {
        $(".loader3").show();
        $.ajax({
            url: '/BuyerDashboard/GetFavouriteProducts',
            type: 'GET',
            success: function (response) {
                $(".loader3").hide();
                $("#ProductsContainer").html(response);
            },
            error: function () {
                toastr.error('An error occurred while loading the product.');
            }
        })
    }


    // for redirection to the selected product
    $(document).on('click', '.card-img', function () {
        var product = $(this).data("product-id");
        window.location.href = '/BuyerDashboard/GetProductsByproductId?productId=' + product;
    });

    $(document).on('click', ".addTofavourite", function () {
        var $this = $(this);
        var id = $this.data("product-id");
        var $icon = $this.find("i");

        $.ajax({
            url: '/BuyerDashboard/UpdateFavourite',
            type: 'POST',
            data: { productId: id },
            success: function (response) {
                if (response.success) {
                    toastr.success("Changes made successfully!", "Success", { timeOut: 4000 });

                    if ($icon.hasClass("bi-heart-fill")) {
                        FetchProducts();
                    } else {
                        FetchProducts();
                    }
                } else {
                    toastr.error('An error occurred while updating favourite.', "Error", { timeOut: 4000 });
                }
            },
            error: function () {
                toastr.error('An error occurred while updating favourite.', "Error", { timeOut: 4000 });
            }
        });
    });

    FetchProducts();
});
