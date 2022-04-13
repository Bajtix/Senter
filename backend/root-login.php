<?php

include 'util.php';

verify_token();

$servername = "localhost";
$username = "dev";
$password = file_get_contents("rootpass.txt");
$db = "software";

// Create connection
$conn = new mysqli($servername, $username, $password, $db);

// Check connection
if ($conn->connect_error) {
    die("Connection failed: " . $conn->connect_error);
}

header('Content-Type: text/json; charset=UTF-8');
