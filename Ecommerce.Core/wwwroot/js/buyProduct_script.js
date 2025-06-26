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


    $(document).on('submit','#SingleOrderDetailsForm',function(e){
        e.preventDefault();
        var data = $(this).serialize();
        console.log(data);
        window.location.href = '/Order/CreatePayment?'+data;
    });
});