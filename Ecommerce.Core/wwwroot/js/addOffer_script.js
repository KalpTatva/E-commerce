$(document).ready(function () {
    function fetchProductName() {
        $.ajax({
            url: '/Dashboard/GetProducts',
            type: 'GET',
            success: function (data) {
                console.log(data);
                var productSelect = $('#ProductId');
                productSelect.empty();
                productSelect.append('<option value="">Select Product</option>');
                $.each(data.products, function (index, product) {
                    productSelect.append($('<option>', {
                        value: product.id,
                        text: product.name
                    }));
                });
            },
            error: function () {
                toastr.error('Failed to load products.');
            }
        });
    }
    fetchProductName();  
});