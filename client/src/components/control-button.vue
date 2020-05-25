<template><div class="cnt">

<button class="btn"><i class="icon-server"></i>IIS</button>
<div class="dropdown">
  <button class="btn" style="border-left:1px solid #0d8bf2">
	
    <i class="icon-arrow-down"></i>
  </button>
  <div class="dropdown-content">
    <a href="#">Link 1</a>
    <a href="#">Link 2</a>
    <a href="#">Link 3</a>
  </div>
</div>

</div></template>

<script lang="ts">
import Vue from "vue";

export default Vue.extend({
	name: "control-button",
});
</script>

<style scoped lang="less">

.icon-arrow-down {
	background-image: url("../assets/arrow.svg");
}
.icon-server {
	background-image: url("../assets/server.svg");
}

i[class^="icon-"] {
    width: 16px;
    height: 16px;
    background-size: contain;
    display: block;
}

.btn {
  background-color: #2196F3;
  color: white;
  padding: 16px;
  font-size: 16px;
  border: none;
  outline: none;
}

.dropdown {
  position: absolute;
  display: inline-block;
}

.dropdown-content {
  display: none;
  position: absolute;
  background-color: #f1f1f1;
  min-width: 160px;
  z-index: 1;
}

.dropdown-content a {
  color: black;
  padding: 12px 16px;
  text-decoration: none;
  display: block;
}

.dropdown-content a:hover {background-color: #ddd}

.dropdown:hover .dropdown-content {
  display: block;
}

.btn:hover, .dropdown:hover .btn {
  background-color: #0b7dda;
}
</style>
