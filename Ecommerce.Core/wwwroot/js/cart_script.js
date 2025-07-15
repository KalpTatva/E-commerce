$(".loader3").show();
$('#CartContainer').hide();
$(document).ready(function () {

    // signalR connection for real-time updates
    // hub connection 
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    // start connection
    connection.start()
        .then(() => console.log("SignalR Connected"))
        .catch(err => {
            console.error(err);
            setTimeout(() => connection.start(), 5000); 
        });

    // receive notification
    connection.on("ReceiveNotification", function (message) {
        //fetcch carts
        FetchCartDetails();
    });

    function FetchCartDetails() {
        $.ajax({
            url: '/BuyerDashboard/GetCart',
            type: 'GET',
            success: function (response) {
                $(".loader3").hide();
                $('#CartContainer').show();
                $("#CartContainer").html(response);
            },
            error: function () {
                toastr.error('An error occurred while getting cart', "Error", { timeOut: 4000 });
            }
        })
    }

    function updateQuantityDetails(cartId, quantity, onSuccess, onError) {
        $.ajax({
            url: '/BuyerDashboard/UpdateValuesOfCart',
            type: 'PUT',
            data: { quantity, cartId },
            success: function (response) {
                if (response.success) {
                    $("#TotalPrice").html(new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR' }).format(response.totalPrice));
                    $("#TotalDiscount").html(new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR' }).format(response.totalDiscount));
                    $("#TotalQuantity").html(response.totalQuantity);
    
                    $("#TotalPrice").data("totalprice", response.totalPrice);
                    $("#TotalDiscount").data("totaldiscount", response.totalDiscount);
                    $("#TotalQuantity").data("totalquantity", response.totalQuantity);
                    onSuccess();
                } else {
                    toastr.error(response.message, "Error", { timeOut: 4000 });
                    onError();
                }
            },
            error: function () {
                toastr.error('An error occurred while updating the cart', "Error", { timeOut: 4000 });
                onError();
            }
        });
    }
    
    function handleQuantityChange(button, isIncrement) {
        const $btn = $(button);
        const $wrapper = $btn.parent();
        const $qtySpan = $wrapper.find('.quantity-text');
        const $minusBtn = $wrapper.find('.Quantity-minus');
        const $plusBtn = $wrapper.find('.Quantity-plus');
    
        let quantity = $btn.data("quantity");
        const cartId = $btn.data("cart-id");
    
        if (!isIncrement && quantity <= 1) {
            toastr.error('Value can\'t be less than 1', "Error", { timeOut: 4000 });
            return;
        }
    
        quantity = isIncrement ? quantity + 1 : quantity - 1;
    
        updateQuantityDetails(
            cartId,
            quantity,
            function () {
                // Success callback: Update UI
                $qtySpan.text(quantity);
                $minusBtn.data("quantity", quantity);
                $plusBtn.data("quantity", quantity);
            },
            function () {
                // Error callback: Do nothing or handle error
            }
        );
    }
    
    $(document).on('click', '.Quantity-minus', function () {
        handleQuantityChange(this, false);
    });
    
    $(document).on('click', '.Quantity-plus', function () {
        handleQuantityChange(this, true);
    });

    $(document).on('click', '.RemoveProductFromCart', function () {
        var cartId = $(this).data('cart-id');
        $.ajax({
            url: '/BuyerDashboard/UpdateCartList',
            type: 'PUT',
            data: {
                cartId: cartId
            },
            success: function (response) {

                $("#CartContainer").html(response);
            },
            error: function () {
                toastr.error('An error occurred while getting cart', "Error", { timeOut: 4000 });
            }
        })
    });



    // pending work


    $(document).on('click', '.buy-all-btn', function () {
        // Collect all data-cart-id values from buttons with class 'BuyProduct'
        var cartIds = $(".BuyProduct").map(function () {
            return $(this).data("cart-id");
        }).get();

        // Serialize to JSON
        var obj = {
            orders: cartIds,
            totalPrice: $("#TotalPrice").data("totalprice"),
            totalDiscount: $("#TotalDiscount").data("totaldiscount"),
            totalQuantity: $("#TotalQuantity").data("totalquantity"),
            isByProductId: false,
        };

        $.ajax({
            url:"/Order/SetSessionForOrder",
            type:"POST",
            data:{objectCart:JSON.stringify(obj)},
            success:function(data){
                if(data.success)
                {
                    window.location.href = '/Order/Index?sessionId='+data.message;
                }
                else
                {
                    toastr.error(data.message, "Error", { timeOut: 4000 });
                }
            },error: function(){
                toastr.error('An error occurred while setting up order', "Error", { timeOut: 4000 });
            }
        })

        

    });
    FetchCartDetails();
});