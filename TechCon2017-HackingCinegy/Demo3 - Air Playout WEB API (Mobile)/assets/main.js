$(document).ready(function() {

	$('.list-wrap').css('height', (window.innerHeight - 170) + 'px');
	// Command Button
	$('#step1 section > div').on('click', function(e) {
		var command = $( this ).attr('data-command');
		if (command != 'Start') {
			var reqType = '<SetOutput State="' + command + '"/>';
		} else if (command == 'Start') {

			var isCandidate = $('.item').is('#candidate');
			
			if ( isCandidate ) {
				var candidateCued = $('#candidate').attr('data-bind');
				$('.item').attr('id', '');
				var reqType = '<Cue Id="{'+ candidateCued +'}"/><StartCued />';
			} else {
				var reqType = '<Cue Id="{'+ statusObj.cued +'}"/><StartCued />';
			}
		}

		var xml = '' +
			'<?xml version="1.0" encoding="utf-8"?>'+
			'<Request>'+
			reqType +
			'</Request>';

		$.ajax({
			url: serverUrl + "/video/command",
			type: "POST",
			contentType: "text/xml",
			processData: false,
			data: xml,
			success: function (res) {
				var answ = $(res).find('Result').attr('Success');
				if (answ == 'y') visualPart(command);
			}
		});
	});

	$('#list').on('click', '.item', function() {
		if ($( this ).attr('id') == 'candidate') {
			$( this ).attr('id','');
		} else {
			$('.item').attr('id','');
			$( this ).attr('id','candidate');
		}
	});

});