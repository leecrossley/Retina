var nicem = (function () {
    var nicem = {};
    pageLoad = {};
	
	nicem.searchTerm = "";
	
	nicem.init = function () {
        setupPageLoadEvent();
        nicem.subscribeToPageLoad("pageLoad", ajaxPageLoad);
    };

    nicem.subscribeToPageLoad = function (name, func) {
        if (typeof func === "function") {
            pageLoad[name] = func;
        }
    };

    function setupPageLoadEvent () {
        $('div[data-role="page"]').live('pagebeforecreate', function() {
            for (var key in pageLoad) {
                if (pageLoad.hasOwnProperty(key)) {
                    pageLoad[key]($(this).attr("id"));
                }
            }
        });
    }
	
    function ajaxPageLoad (id) {
		if (id === "image1") {
			initCrop(id);
			attachNextHijack(id);
		}
    }
	
	function attachNextHijack (id) {
		$('a[data-next-button="true"]').bind("click", function(e) {
			e.preventDefault();
			e.stopPropagation();
			$("." + id).hide();
			$.mobile.showPageLoadingMsg();
			var data = {};
			data.x = $('#x').val();
			data.y = $('#y').val();
			data.w = $('#w').val();
			data.h = $('#h').val();
			$.ajax({
				type: "GET",
				url: "http://www.ukfy.co.uk/image",
				data: data,
				success: function (data) {
					$.mobile.hidePageLoadingMsg();
					if (data.success) {
						alert(data.result);
						nicem.searchTerm = data.result;
						$.mobile.loadPage("process.html");
					} else {
						alert("unable to process your request on this occassion.");
					}
				},
				error: function () {
					$.mobile.hidePageLoadingMsg();
					alert("unable to process your request on this occassion.");
				},
				dataType: "jsonp",
				crossDomain: true
			});
			return false;
		});
	}
	
	function initCrop (id) {
		$("." + id).Jcrop({
			onChange: setCoordVals,
			onSelect: setCoordVals
		});
	}
	
	function setCoordVals(c) {
		$('#x').val(c.x);
		$('#y').val(c.y);
		$('#w').val(c.w);
		$('#h').val(c.h);
	};
	
    return nicem;
})();

$(document).ready(function () {
    nicem.init();
});

$(document).bind("mobileinit", function(){
  $.extend($.mobile, {
      allowCrossDomainPages: true
  });
});

