$(".loader3").hide();

$(document).ready(function(){

    var deleteProductModal = new bootstrap.Modal(document.getElementById('ProductDeleteModal'), {
        keyboard: false,
        backdrop: 'static'
    });


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
            error: function () {
                toastr.error('An error occurred while loading the product.');
            }
        });
    }

    $(document).on('click','.product-delete-btn',function(){
        var product = $(this).data('product-id');
        $('#deleteProductId').val(product);
        deleteProductModal.show();
    });

    $(document).on('submit','#productDeleteForm', function(e){
        e.preventDefault();
        $.ajax({
            url: '/Product/DeleteProduct',
            type: 'PUT',
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message,"Success",{timeOut:5000});
                    FetchProductsDetails();
                    deleteProductModal.hide();
                } else {
                    toastr.error(response.message,"Error",{timeOut:5000});
                }
            },
            error: function (xhr, status, error) {
                toastr.error('An error occurred while delete the product.');
            }
        })
    })


    FetchProductsDetails();
});