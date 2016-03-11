// JS methods for working wioth Cinegy Type

	function StartTemplate( templateName ) {
		var xmlhttp = new XMLHttpRequest();
		xmlhttp.open('POST', 'http://localhost:5521/video/command', false);
		xmlhttp.setRequestHeader('Content-Type', 'text/xml');

		var Op1 = templateName;
		var Op2 = '<variables><var name="Left-Height" type="Float" value="800"/><var name="Right-Height" type="Float" value="800"/><var name="Left-Y" type="Float" value="0"/><var name="Right-Y" type="Float" value="0"/><var name="MaskRightY" type="Float" value="600"/><var name="MaskLeftY" type="Float" value="300"/><var name="LeftValue" type="Text" value="20%"/><var name="RightValue" type="Text" value="60%"/></variables>';

		Op2 = Op2.replace(/&/g, '&amp;').replace (/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
		
		xmlhttp.send('<?xml version="1.0" encoding="utf-8"?> <Request> <Event Device="*CG_0" Cmd="SHOW" Op1="'+Op1+'" Op2="'+Op2+'" Op3="" Name="" Description="" ThirdPartyId=""> </Event> </Request>');

	}
	
	function SetVariable( name, type, value) {
		var xmlhttp = new XMLHttpRequest();
		xmlhttp.open('POST', 'http://localhost:5521/postbox', false);
		var str = '﻿<?xml version="1.0" encoding="utf-8"?><PostRequest><SetValue Name="'+name+'" Type="'+type+'" Value="'+value+'" /> </PostRequest>';
		xmlhttp.setRequestHeader('Content-Type', 'text/xml');
		xmlhttp.send(str);
		var text = xmlhttp.responseText;
	}

	function SetVariables( varArray ) {
		
	}
