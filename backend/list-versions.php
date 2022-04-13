<?php
include "login.php";

if (!is_numeric($_GET["app"])) {
    die("invalid app");
}

$sql = "SELECT * FROM `app" . $_GET["app"] . "` ";

$result = $conn->query($sql);

echo ($conn->error);

$data_raw = $result->fetch_all(MYSQLI_ASSOC);
$json = json_encode($data_raw, JSON_PRETTY_PRINT);

echo ($json);
