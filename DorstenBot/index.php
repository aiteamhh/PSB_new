<?php
session_start();
ini_set('display_errors',1);
$pwhash = '61972152f19924b2c5f92fe20f019741';
?>

<html>
<head>
    <title></title>
    <meta charset="utf-8" />	
	<link href="content/page/botchat.css" rel="stylesheet" />
	<script src="scripts/md5.min.js"></script>

	<?php
	if (isset($_POST['login']) && md5($_POST['login']) == $pwhash)
	{
	?>
		<style>
			body {
				font-family:'Segoe UI';
				background-image: url('content/page/background.PNG');
				background-repeat: no-repeat;
			}
			
			#botcontent {
				margin-left: 430px;
				margin-top: 420px;
			}
			
			#bot {
				width: 650px; 
				height: 720px; 
				margin-bottom: -5px;
				position: relative !important; 
				border: solid 2px #dfdfdf;
			}
		</style>
	<?php 
	}
	?>
</head>
<body>
	<?php 
	if (isset($_POST['login']) && md5($_POST['login']) == $pwhash)
	{
		?>
			<!-- <img src='content/page/db_logo.png' style='height: 50px; width: 300px'> -->
			<div id='botcontent'>	
				<h3>Willkommen</h3>
				<img src='content/page/avatar1.png' style='margin-top: 450px; margin-left:25px; position:absolute' />
				<div id='bot' />
			</div>
			
			<script src='content/page/botchat.js'></script>
			<script>			
				BotChat.App({
					directLine: { secret: '' },
					user: { id: 'userid' },
					bot: { id: '' },
					resize: 'detect' 
				  }, document.getElementById('bot'));
			</script>
			<div style='margin-top: 800px' />
		<?php 
	}
	else
	{
		?>
			<script>
			  function onSubmit(){
				  var pw = $('#pwField').val();
				  if (md5(pw) == '61972152f19924b2c5f92fe20f019741'){
					$('#wrongPw').css('display','none');
					$.ajax({
						type: 'POST',
						url: 'index.php',
						data: 'login=t',
						success: function (data) {
							alert("Success");
						}
					});
				  }
				  else
				  {
					  $('#wrongPw').css('display','block');
				  }
			  }
			</script>
		
		<form action='index.php' method='POST' id='loginform' style='margin-top:150px; margin-left: 150px'>
			<h4>Geben Sie bitte das Passwort ein</h4>
			<input type='password' id='pwField' name="login" /><br>
			<input type='submit' style='margin-top: 10px' value='Abschicken' />
			<?php 
			if (isset($_POST['login'])) {
				echo "<p id='wrongPw' style=' color: #f00'>Falsches Passwort eingegeben</p>";
			}
			?>
		</form>
	<?php
	}
	?>
</body>
</html>

 