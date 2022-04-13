<?php
include "login.php";

$sql = "SELECT * FROM `software` ";

$result = $conn->query($sql);



$data_raw = $result->fetch_all(MYSQLI_ASSOC);

$json = json_encode($data_raw, JSON_PRETTY_PRINT);

echo ($json);
