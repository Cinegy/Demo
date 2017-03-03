$(document).ready(function() {

	// Command Button
	$('#step1 section > div').on('click', function(e) {
		var command = $( this ).attr('data-command');

		if (command != 'Start') {
			var reqType = '<SetOutput State="'+command+'"/>';
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
			url: "http://localhost:5521/video/command",
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

	var visualPart = function(command) {
		$('.control-panel li span').removeClass('off');
		if ( command == 'Black' ) $('.control-panel li').eq(3).find('span').addClass('off');
		if ( command == 'Bypass' ) $('.control-panel li').eq(2).find('span').addClass('off');
		if ( command == 'Clean' ) $('.control-panel li').eq(1).find('span').addClass('off');
	}

	this.getStatus = function(e) {
		$('.lined').removeClass('check');
		$( e.target ).addClass('check');
		$('#step1').animate({'top': '85%'}, 800);
		$('#pagination > li').removeClass('check');
		$('#pagination > li').eq(1).addClass('check');
	}


	$('#pagination > li').on('click', function() {
		var ident = $(this).attr('data-ident');
		var prevIdent = $('#pagination > li.check').attr('data-ident');

		switch (ident) {
			case '1':
					window.location.reload();
				break;
			case '2':
					$('#pagination > li').removeClass('check');
					$(this).addClass('check');
				break;
			case '3':
					$('#pagination > li').removeClass('check');
					$(this).addClass('check');

					$('#step3, .left-wrap').animate({'width':'50%'}, 1000);
					$('.vertical').stop().animate({'height':'100%'}, 1000);
				break;
			default:
					console.log( 'error' );
		}
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