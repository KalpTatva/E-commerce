$(document).ready(function(){
    $(document).on('submit','#BackToDashBoardForm',function(e){
        $.ajax({
            url: '/Order/CancelOrderBefore',
            type: 'DELETE',
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    window.location.href = '/BuyerDashboard/Index'
                } else {
                    toastr.error(response.message);
                }
            },
            error: function (xhr, status, error) {
                toastr.error('An error occurred while delete the product.');
            }
        })
    });


    $(document).on('submit','#OrderDetailsForm',function(e){
        e.preventDefault();
        $.ajax({
            url: '/Order/PlaceOrder',
            type: 'POST',
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    window.location.href = '/Dashboard/MyOrders';
                } else {
                    toastr.error(response.message);
                }
            },
            error: function (xhr, status, error) {
                toastr.error('An error occurred while delete the product.');
            }
        })
    });

});
