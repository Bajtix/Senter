<?php
$servername = "localhost";
$username = "userapi";
$password = file_get_contents("apipass.txt");
$db = "senter";

// Create connection
$conn = new mysqli($servername, $username, $password, $db);

// Check connection
if ($conn->connect_error) {
    die("Connection failed: " . $conn->connect_error);
}

header('Content-Type: text/json; charset=UTF-8');
