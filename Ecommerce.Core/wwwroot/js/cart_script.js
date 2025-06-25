$(".loader3").show();
$('#CartContainer').hide();
$(document).ready(function () {

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

    function updateQuantityDetails(cartId, quantity) {
        $.ajax({
            url: '/BuyerDashboard/UpdateValuesOfCart',
            type: 'PUT',
            data: {
                quantity: quantity,
                cartId: cartId
            },
            success: function (response) {
                if (response.success) {
                    $("#TotalPrice").html(new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR' }).format(response.totalPrice));
                    $("#TotalDiscount").html(new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR' }).format(response.totalDiscount));
                    $("#TotalQuantity").html(response.totalQuantity); // Quantity is just a number

                    $("#TotalPrice").data("totalprice", response.totalPrice);
                    $("#TotalDiscount").data("totaldiscount", response.totalDiscount);
                    $("#TotalQuantity").data("totalquantity", response.totalQuantity);

                } else {
                    toastr.error('An error occurred while getting cart', "Error", { timeOut: 4000 });
                }
            },
            error: function () {
                toastr.error('An error occurred while getting cart', "Error", { timeOut: 4000 });
            }
        })
    }

    $(document).on('click', '.Quantity-minus', function () {
        var $minusBtn = $(this);
        var $wrapper = $minusBtn.parent(); // The parent <span class="d-flex gap-3 ">
        var $qtySpan = $wrapper.find('.quantity-text');
        var $plusBtn = $wrapper.find('.Quantity-plus');

        var quantity = $(this).data("quantity");
        var cartId = $(this).data("cart-id");
        if (quantity <= 1) {
            toastr.error('value can\'t be less than 1', "Error", { timeOut: 4000 });
            return;
        }

        quantity--;

        // Update UI
        $qtySpan.text(quantity);
        $minusBtn.data("quantity", quantity);
        $plusBtn.data("quantity", quantity);

        updateQuantityDetails(cartId, quantity);
    });

    $(document).on('click', '.Quantity-plus', function () {
        var $plusBtn = $(this);
        var $wrapper = $plusBtn.parent();
        var $qtySpan = $wrapper.find('.quantity-text');
        var $minusBtn = $wrapper.find('.Quantity-minus');

        var quantity = $(this).data("quantity");
        var cartId = $(this).data("cart-id");

        quantity++;

        // Update UI
        $qtySpan.text(quantity);
        $plusBtn.data("quantity", quantity);
        $minusBtn.data("quantity", quantity);

        updateQuantityDetails(cartId, quantity);
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
            totalQuantity: $("#TotalQuantity").data("totalquantity")
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