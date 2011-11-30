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
		} else if (id === "results") {
			performSearch();
		}
    }
	
	function attachNextHijack (id) {
		$('a[data-next-button="true"]').bind("click", function(e) {
			e.preventDefault();
			e.stopPropagation();
			$("." + id).hide();
			$(".jcrop-holder").css("visibility", "hidden");
			$.mobile.showPageLoadingMsg();
			var url = "http://www.ukfy.co.uk/image?x=" + $('#x').val() + "&y=" + $('#y').val() + "&w=" + $('#w').val() + "&h=" + $('#h').val();
			$.ajax({
				beforeSend: function (xhr) {
					xhr.setRequestHeader("Accept", "application/json");
				},
				url: url,
				success: function (data) {
					$.mobile.hidePageLoadingMsg();
					if (data.success) {
						nicem.searchTerm = data.result;
						$.mobile.changePage("results.html");
					} else {
						alert("Unable to process image.");
					}
				},
				error: function () {
					$.mobile.hidePageLoadingMsg();
					alert("Unable to process image.");
				}
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
	
	function performSearch() {
		var url = "https://api.nice.org.uk/services/search/results?q=" + nicem.searchTerm;
		$.ajax({
			url: url,
			beforeSend: function (xhr) {
				xhr.setRequestHeader('Accept', 'application/json');
				xhr.setRequestHeader('API-Key', '4b9eae0d-a570-43d8-929f-308bf20991de');
			},
			success: function (data, textStatus, jqXhr) {
				$('#resultslist').empty();
				$.each(data.Documents, function (){
					var doc = this;
					var result = "<li class='ui-li ui-li-static ui-body-a'>" +
						"<h3 class='ui-li-heading'>" + doc.Title + "</h3>" +
						"<p class='ui-li-desc'>" + doc.Abstract + "</p>" +
						"</li>";
					$('#resultslist').append(result);
				});
			},
			error: function () {
				alert("Error getting data from syndication.");
			}
		});
		return false;
	}
	
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

