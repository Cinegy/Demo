
// Server adress

var serverUrl = 'http://localhost:5521';
/*#####################################*/


var xhr = new XMLHttpRequest();
var statusUrl = serverUrl + '/video/status';
var response, statusObj;
var iterCount = 0;
var iterCountTest = 0;

getStatus( statusUrl );

// check status at intervals of 1000 ms
setInterval( function() { getStatus( statusUrl ) }, 1000);

	// get status
	function getStatus( statusUrl ) {
		xhr.open('GET', statusUrl , true);
		xhr.send();
		xhr.onreadystatechange = function() {
			if (xhr.readyState != 4) return;
			if (xhr.status != 200) {
				var onAir = document.getElementById('onAir');
					onAir.className = 'off-air';
			} else {
				response = xhr.responseText;
				parseXML(response, function(jsonObj) {
					if (jsonObj) {

						statusObj = {
							active: jsonObj.Status.Active._Id.replace(/}|{/g,''),
							cued : jsonObj.Status.Cued._Id.replace(/}|{/g,''),
							license : jsonObj.Status.License._State,
							output : jsonObj.Status.Output._State
						}

						if ( statusObj ) {
							bindStatus( statusObj );
							getList(statusObj);
						} else {
							var onAir = document.getElementById('onAir');
								onAir.className = 'off-air';
						}
					}
				});
			}
		}
	}

	// get item list
	var getList = function(statusObj) {
		var listUrl = serverUrl + '/video/list';
		xhr.open('GET', listUrl, true);
		xhr.send();
		xhr.onreadystatechange = function() {
		  if (xhr.readyState != 4) return;
		  if (xhr.status != 200) {
		    console.log(xhr.status + ': ' + xhr.statusText);
		  } else {
		    parseXML(xhr.responseText, function(jsonObj) {
		    	var listArr = jsonObj.List.Item;
		    	bindList(listArr, statusObj);
		    });
		  }
		}
	}

/*------------------------------------------------------*/

	// parse XML request 
	var parseXML = function(response, callback) {
		var x2js = new X2JS();
		var jsonObj = x2js.xml_str2json( response );

		callback(jsonObj);
	}

	var bindStatus = function(statusObj) {
		document.getElementById('active').innerText = statusObj.active;
		document.getElementById('cued').innerText = statusObj.cued;
		document.getElementById('license').innerText = statusObj.license;
		document.getElementById('output').innerText = statusObj.output;

		var onAir = document.getElementById('onAir');
			onAir.className = 'on-air';
	}

	var bindList = function(listArr, statusObj) {

		$('.item').addClass('ident');
		if (iterCount != 0) var itemList = $('#list .item');

		for(var j=0; listArr.length > j; j++){

			if (iterCount == 0 ) {
				
				var newElem = '<div class="item" data-count="' + j + '" data-bind="'+listArr[j]._Id.replace(/}|{/g,'')+'"><span>'+
					listArr[j]._ScheduledAt+'</span>'+
					'<span class=""><i></i></span><span>' + 
					listArr[j]._Name+'</span><span>'+
					listArr[j]._Description+'</span><span>'+
					listArr[j]._Duration+'</span></div>';

				$('#list').append(newElem);

			} else {
				var specClass = '';
				var loopClass = '';
				var bindIdent = listArr[j]._Id.replace(/}|{/g,'');
				var iterElem = $('.item[data-bind=' + bindIdent + ']');

				if ( iterElem.length == 0 ) {
					var newElem = '<div class="item" data-count="' + j + '" data-bind="'+listArr[j]._Id.replace(/}|{/g,'')+'"><span>'+
					listArr[j]._ScheduledAt+'</span>'+
					'<span class=""><i></i></span><span>' + 
					listArr[j]._Name+'</span><span>'+
					listArr[j]._Description+'</span><span>'+
					listArr[j]._Duration+'</span></div>';
					$('.item').eq(j-1).after(newElem);
				}

				if ( bindIdent == statusObj.active ) specClass = 'active-item';
				if ( bindIdent == statusObj.cued ) specClass = 'cued-item';
				if (listArr[j]._LoopBody && listArr[j]._LoopBody == 'y') {
					var loopClass = 'loop';
				} else if (listArr[j]._LoopStart) {
					var loopClass = 'loop-start';
				}
				var loop = listArr[j]._LoopBody;
				if ( loop == 'y' ) {

					var loopNext = listArr[j+1] ? listArr[j+1]._LoopBody : undefined;
					if ( loopNext == undefined ) var loopClass = 'loop-end';
				}
				iterElem.removeClass('ident');
				iterElem.attr('class', loopClass + ' item ' + specClass);
			}
		}

		$('.item.ident').remove();
		iterCount++;
	}