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

    // summer note
    $('#Description').summernote({
        height: 200,
        placeholder: 'Type your description here...it should be detailed and informative which can make impact on customer',
        toolbar: [
          ['style', ['bold', 'italic', 'underline', 'clear']],
          ['font', ['strikethrough', 'superscript', 'subscript']],
          ['fontsize', ['fontsize']],
          ['color', ['color']],
          ['para', ['ul', 'ol', 'paragraph']],
          ['height', ['height']]
        ]
    });
    fetchProductName();  
});