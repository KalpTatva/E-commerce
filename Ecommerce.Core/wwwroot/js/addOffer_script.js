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

    // time picker
    $('.timepicker').timepicker({
        timeFormat: 'h:mm p',
        interval: 30,
        minTime: '6:00am',
        maxTime: '11:00pm',
        defaultTime: '9:00am',
        StartTime: '6:00am',
        dynamic: false,
        dropdown: true,
        scrollbar: true
    });


    // summer note
    $('#Description').summernote({
        height: 100,
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