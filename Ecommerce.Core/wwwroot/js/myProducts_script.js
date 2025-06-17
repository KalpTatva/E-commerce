$(".loader3").hide();

$(document).ready(function(){
    function FetchProductsDetails()
    {
        $(".loader3").show();
        $.ajax({
            url: '/Product/GetSellerSpecificProducts',
            type: 'GET',
            success: function (response) {
                $(".loader3").hide();
                $("#productDetails").html(response);
            },
            error: function (xhr, status, error) {
                toastr.error('An error occurred while loading the product.');
            }
        });
    }
    FetchProductsDetails();
});