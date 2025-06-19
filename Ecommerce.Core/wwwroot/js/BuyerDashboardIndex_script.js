$(".loader3").hide();

$(document).ready(function () {
    let categoryInput= null;
    let searchInput = null;

    // function for getting product on page
    function FetchProducts(categoryInput, searchInput) {
        $(".loader3").show();
        categoryInput = new URLSearchParams(window.location.search).get('categoryId');
        console.log(categoryInput, "input is");
        $.ajax({
            url: '/BuyerDashboard/GetProducts',
            type: 'GET',
            data: {
                search : searchInput,
                category : categoryInput
            },
            success: function (response) {
                $(".loader3").hide();
                $("#ProductsContainer").html(response);
            },
            error: function () {
                toastr.error('An error occurred while loading the product.');
            }
        })
    }


    $(document).on('input','#searchInput',function(){
        searchInput = $(this).val();
        FetchProducts(categoryInput, searchInput);

    })



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
                        $icon.removeClass("bi-heart-fill").addClass("bi-heart");
                        $this.attr("data-is-favourite", "false");
                    } else {
                        $icon.removeClass("bi-heart").addClass("bi-heart-fill");
                        $this.attr("data-is-favourite", "true");
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


    $(document).on('click', '.AddToCart', function (e) {
        e.preventDefault();
        var productId = $(this).data("product-id");
        $.ajax({
            url: '/BuyerDashboard/AddToCart',
            type: 'POST',
            data: { productId: productId },
            success: function (response) {
                if (response.success) {
                    window.location.href = '/BuyerDashboard/Cart';
                }
                else {
                    toastr.error(response.message, "Error", { timeOut: 4000 });
                }
            },
            error: function () {
                toastr.error('An error occurred while updating cart', "Error", { timeOut: 4000 });
            }
        })
    })

    FetchProducts(categoryInput, searchInput);
});