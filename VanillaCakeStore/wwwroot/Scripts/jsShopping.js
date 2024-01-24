
$(document).ready(function () {
    ShowCount();
    initQuantity();
    $('body').on('click', '.btnAddToCart', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        var quantity = 1;
        var tQuantity = $('#quantity_value').text();
        if (tQuantity != '') {
            quantity = parseInt(tQuantity);
        }
/*        alert(id + '     ' + quantity);*/

        $.ajax({
            url: '/shoppingcart/addtocart',
            type: 'POST',
            data: { id: id, quantity: quantity },
            success: function (rs) {
                if (rs.Success) {
                    $('#cart_cout').html(rs.Count);
                    alert(rs.msg);
                }
                else {
                    alert(rs.msg);
                }
            }
        });
    });

    $('body').on('click', '.btnDelete', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        var conf = confirm('Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng?');
        if (conf == true) {
            $.ajax({
                url: '/shoppingcart/Delete',
                type: 'POST',
                data: { id: id },
                success: function (rs) {
                    if (rs.Success) {
                        $('#cart_cout').html(rs.Count);
                        $('#trow_' + id).remove();
                        LoadCart();
                    }
                }
            });
        }

    });
    //$('.txtQuantity').off('keyup').on('keyup', function () {

    //    var quantity = parseInt($(this).val());
    //    var productid = parseInt($(this).data('id'));
    //    var price = parseFloat($(this).data('price'));
    //    if (isNaN(quantity) == false) {

    //        var amount = quantity * price;
    //        $('#amout_' + productid).text(amount +' Đ');
    //    }
    //    else {
    //        $('#amout_' + productid).text(0);
    //    }
    //            $('#lblTotalOrder').text(GetTotalOrder() + ' Đ');
    //    Update(productid, quantity);
    //});
    var timeout = null;
    $('body').off('keyup').on('keyup', '.txtQuantity', function (e) {
        clearTimeout(timeout);
        e.preventDefault();
        var quantity = parseInt($(this).val());
        var productid = parseInt($(this).data('id'));
        var price = parseFloat($(this).data('price'));
        timeout = setTimeout(function () {
            console.log('sau 10 giay');

            Update(productid, quantity);
        }, 400);

    });

    //$('body').on('keyup', '.txtQuantity', function (e) {
    //    e.preventDefault()
    //    setTimeout(function () {
    //        console.log('sau 2 giay');
    //        alert("sau 5 giay");
    //    }, 2000);
    //});
});

//function GetTotalOrder() {
//    var listTextBox = $('.txtQuantity');
//    var total = 0;
//    $.each(listTextBox, function (i, item) {
//        total += parseInt($(item).val()) * parseFloat($(item).data('price'));
//    });
//    return total;
//}

function ShowCount() {
    $.ajax({
        url: '/shoppingcart/ShowCount',
        type: 'GET',
        success: function (rs) {
            $('#cart_cout').html(rs.Count);
        }
    });
}

function Update(id, quantity) {
    $.ajax({
        url: '/shoppingcart/Update',
        type: 'POST',
        data: { id: id, quantity: quantity },
        success: function (rs) {
            if (rs.Success) {
                LoadCart();
            }
            else {
                alert("Số lượng trong kho không đủ, xin mời bạn nhập lại !!!");
            }
        }
    });


    //alert('update');
    //LoadCart();
}
function LoadCart() {
    $.ajax({
        url: '/shoppingcart/Partial_Item_Cart',
        type: 'GET',
        success: function (rs) {
            $('#load_data').html(rs);
        }
    });


/*    alert('loadcart');*/
}


//sua so luong trang chi tiet
function initQuantity() {
    if ($('.plus').length && $('.minus').length) {
        var plus = $('.plus');
        var minus = $('.minus');
        var value = $('#quantity_value');

        plus.on('click', function () {
            var x = parseInt(value.text());
            value.text(x + 1);
        });

        minus.on('click', function () {
            var x = parseInt(value.text());
            if (x > 1) {
                value.text(x - 1);
            }
        });
    }
}