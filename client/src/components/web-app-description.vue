<template><div  class="web-app-cnt">
<div class="app-control-panel">
	<div class="app-info-cnt">
		<a :href="app.path" style="text-decoration: initial;"><span class="app-caption">{{app.path}}</span></a>
	</div>
	<div class="actions-cnt">

		<el-tooltip v-if="applicationPoolInfo" class="item" effect="light" placement="left"
			:disabled="!applicationPoolInfo.existProcesses">
			<div slot="content"><span class="application-processes-description" v-html="applicationPoolInfo.processesDescription"></span></div>
			<el-tag style="margin-right: 8px;" :type="applicationPoolInfo.type"
				disable-transitions :effect="applicationPoolInfo.effect">{{applicationPoolInfo.state}}</el-tag>
		</el-tooltip>

		<el-button type="info" size="small"  :plain="displayMode != DisplayMode.SQL"
				@click="setDisplayMode(DisplayMode.SQL)">SQL</el-button>

		<el-button type="info" size="small"  style="margin-left:10px" :plain="displayMode != DisplayMode.REDIS"
				@click="setDisplayMode(DisplayMode.REDIS)">REDIS</el-button>

		<el-button type="info" size="small" style="margin-left:10px" :plain="displayMode != DisplayMode.DETAILS"
			@click="setDisplayMode(DisplayMode.DETAILS)"><i class="el-icon-tickets" /></el-button>

		<el-dropdown style="margin-left:10px" @command="handleCommand($event)">
			<el-button type="danger" plain size="small"><i class="el-icon-more-outline"></i></el-button>
			<el-dropdown-menu slot="dropdown">
				<el-dropdown-item command="clearRedis"><i class="el-icon-delete"></i>Очистити redis</el-dropdown-item>
			</el-dropdown-menu>
		</el-dropdown>
	</div>
</div>

</div></template>

<script lang="ts">

import Vue from "vue";
import { WebApp } from "../../../api-client/index";
import { Component, Prop } from "vue-property-decorator";


@Component
export default class WebAppDescription extends Vue {

	@Prop({required: true})
	public app: WebApp;

}

</script>

<style scoped lang="less">

</style>
