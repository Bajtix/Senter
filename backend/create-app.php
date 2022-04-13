<?php
include 'root-login.php';


$sql = "INSERT INTO `software` (`id`, `name`, `description`, `icon`, `link`) VALUES (NULL, '', '', '', '')"; // create new app in list
$conn->query($sql);
$sql = "SELECT max(id) FROM software"; // get last id
$response = $conn->query($sql);

$id = $response->fetch_all()[0][0]; // get max id
echo ($id);

$sql = "CREATE TABLE app" . $id . " LIKE app";
$conn->query($sql);
