$(".sendMessage").show();
$(".loader2").hide();
$(document).ready(function () {
  function fetchProductName() {
    $.ajax({
      url: "/Dashboard/GetProducts",
      type: "GET",
      success: function (data) {
        console.log(data);
        var productSelect = $("#ProductId");
        productSelect.empty();
        productSelect.append('<option value="">Select Product</option>');
        $.each(data.products, function (index, product) {
          productSelect.append(
            $("<option>", {
              value: product.id,
              text: product.name,
            })
          );
        });
      },
      error: function () {
        toastr.error("Failed to load products.");
      },
    });
  }
  $(document).on("submit", "#contactUsForm", function () {
    $(".sendMessage").hide();
    $(".loader2").show();
  });

  $('#MessageInput').summernote({
    height: 200,
    placeholder: 'Type your message here...',
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
