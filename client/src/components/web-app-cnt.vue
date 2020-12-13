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

			<el-button type="info" size="small" :plain="displayMode != DisplayMode.SQL"
					@click="setDisplayMode(DisplayMode.SQL)">SQL</el-button>

			<el-button type="info" size="small"  style="margin-left:10px" :plain="displayMode != DisplayMode.REDIS"
					@click="setDisplayMode(DisplayMode.REDIS)">REDIS</el-button>

			<el-button type="info" size="small" style="margin-left:10px" :plain="displayMode != DisplayMode.DETAILS"
				@click="setDisplayMode(DisplayMode.DETAILS)"><i class="el-icon-tickets" /></el-button>

			<el-dropdown style="margin-left:10px" @command="handleCommand($event)">
				<el-button type="danger" plain size="small"><i class="el-icon-more-outline"></i></el-button>
				<el-dropdown-menu slot="dropdown">
					<el-dropdown-item v-for="command in Commands" :key="command.code" :command="command.code"
						><i :class="command.icon"></i>{{command.caption}}</el-dropdown-item>
				</el-dropdown-menu>
			</el-dropdown>

		</div>

	</div>

	<div class="app-wrap-cnt" v-if="displayMode == DisplayMode.DETAILS">
		<el-table :data="webAppDetails" :show-header="false">
			<el-table-column prop="property" class-name="app-property-name" width="175px"></el-table-column>
			<el-table-column prop="value" class-name="app-property-value"></el-table-column>
		</el-table>
	</div>

	<web-app-sql-panel v-else-if="displayMode == DisplayMode.SQL" :app="app"></web-app-sql-panel>

	<div class="app-wrap-cnt" v-if="displayMode == DisplayMode.REDIS">
		<div class="site-console-panel">
			<div>
				<el-button type="primary" size="small" @click="getRedisAll">Get all values</el-button>
				
				
			</div>
		</div>
		<div  class="site-console-content"  style="margin-top: 5px;">
			<el-row :gutter="20">
				<el-col :span="8"><el-input v-model="redisKey" size="small" placeholder="key"></el-input></el-col>
				<el-col :span="4"><el-button type="primary" size="small" @click="getRedisValue">Get</el-button></el-col>
				<el-col :span="8"><el-input v-model="redisValue" size="small" placeholder="value"></el-input></el-col>
				<el-col :span="4"><el-button type="primary" size="small" @click="setRedisValue" :disabled="!redisKey">Set</el-button></el-col>
			</el-row>
			<el-table v-if="redisValues.length > 0"  style="margin-top: 5px;" :data="redisValues" size="small" max-height="300" border
					@row-click="onRedisValuesTableClick">
				<el-table-column show-overflow-tooltip label="Key" prop="Key"></el-table-column>
				<el-table-column show-overflow-tooltip label="Value" prop="Value"></el-table-column>
			</el-table>
		</div>
	</div>

</div></template>

<script lang="ts">
import Vue from "vue";
import store from "../store/index";
import { WebApp, ApplicationPoolData,
	RedisApi, ApplicationPoolApi, StringStringKeyValuePair } from "../../../api-client/index";
import { Component, Prop } from "vue-property-decorator";
import { State } from "vuex-class";

// tslint:disable-next-line
import WebAppSqlPanel from "./web-app-sql-panel.vue";

enum DisplayMode {
	NONE = "",
	DETAILS = "details",
	SQL = "sql",
	REDIS = "redis",
}

interface IApllicationPoolShowInfo {
	state: string;
	existProcesses: boolean;
	processesDescription: string;
	type: string;
	effect: string;
}

const ApplicationPoolStateMap: Record<string, string> = {
	Starting: "",
	Started: "success",
	Stopping: "warning",
	Stopped: "danger",
	Unknown: "info",
};

interface WebAppCommand {
	code: string;
	caption: string;
	icon: string;
	action: (app: WebApp) => Promise<any>;
}

const Commands: WebAppCommand[] = [ {
	code: "clearRedis", caption: "Очистити redis", icon: "el-icon-delete",
	action(app) { return new RedisApi().flushDb(app.id); },
}, {
	code: "stopAppPool", caption: "Зупинити пул", icon: "el-icon-video-pause",
	action(app) { return new ApplicationPoolApi().stop(app.applicationPoolName); },
}, {
	code: "restartAppPool", caption: "Перезапустити пул", icon: "el-icon-refresh-right",
	action(app) { return new ApplicationPoolApi().recycle(app.applicationPoolName); },
}, {
	code: "startAppPool", caption: "Запустити пул", icon: "el-icon-video-play",
	action(app) { return new ApplicationPoolApi().start(app.applicationPoolName); },
},
];

@Component({
	components: {
		WebAppSqlPanel,
	},
})
export default class WebAppCnt extends Vue {

	@Prop({required: true})
	public app: WebApp;

	public redisKey: string = "";
	public redisValue: string = "";

	public Commands = Commands;

	protected DisplayMode = DisplayMode;


	protected redisValues: StringStringKeyValuePair[] = [];

	private displayMode: DisplayMode = DisplayMode.NONE;

	@State("appPools")
	private appPools: ApplicationPoolData[];

	public async handleCommand(commandCode: string) {
		let command = Commands.find((item) => item.code === commandCode);
		if (command == null) {
			throw new Error("Unknown command");
		}
		command = command!;
		if (await this.confirmCommand(command)) {
			await command.action(this.app);
			this.$message({ type: "success", message: "SUCCESS" });

		}
	}

	public async confirmCommand(command: WebAppCommand): Promise<boolean> {
		try {
			await this.$confirm(command.caption + "?", "Warning",
				{ confirmButtonText: "OK", cancelButtonText: "Cancel", type: "warning" });
			return true;
		} catch (err) {
			this.$message({ type: "info", message: "canceled" });
			return false;
		}
	}

	protected async getRedisAll() {
		this.redisValues = await new RedisApi().getAll(this.app.id);
	}

	protected async getRedisValue() {
		this.redisValue = await new RedisApi().getByKey(this.app.id, this.redisKey);
		this.redisValues = [{ Key: this.redisKey, Value: this.redisValue } as StringStringKeyValuePair];
	}

	protected async setRedisValue() {
		await new RedisApi().set(this.app.id, this.redisKey, this.redisValue);
		this.getRedisValue();
	}


	protected setDisplayMode(mode: DisplayMode): void {
		this.displayMode = this.displayMode === mode ? DisplayMode.NONE : mode;
		localStorage.setItem(`${this.app.id}_displayMode`, this.displayMode);
	}

	protected mounted() {
		const value = localStorage.getItem(`${this.app.id}_displayMode`);
		this.setDisplayMode(value ? value as DisplayMode : DisplayMode.NONE);
	}

	protected get applicationPool() {
		return this.appPools.find(({name}) => name === this.app.applicationPoolName)!;
	}

	protected getApplicataionPoolRAMUsage() {
		const value = this.applicationPool.workerProcesses
			.reduce((total, item) => total + Number(item.ram), 0);
		return value > 0 ? ` RAM: ${Math.round(value / 1024)} Mb` : "";
	}

	protected getApplicataionPoolCPUUsage() {
		const value = this.applicationPool.workerProcesses
			.reduce((total, item) => total + Number(item.cpu), 0);
		return value > 0 ? ` CPU: ${value}%` : "";
	}

	protected getApplicationPoolState(): string {
		return this.applicationPool.state +
			this.getApplicataionPoolCPUUsage() +
			this.getApplicataionPoolRAMUsage();
	}

	protected onRedisValuesTableClick(value: StringStringKeyValuePair) {
		const item = value as ({Key: string, Value: string});
		this.redisKey = value.key || item.Key;
		this.redisValue = value.value || item.Value;
	}

	protected getWorkerProcessDescription(): string {
		return this.applicationPool.workerProcesses.map(
			(item) => `Process. Id: <b>${item.processId}</b>. RAM: ${item.ram} Kb. CPU: ${item.cpu}%. ${item.state}`,
		).join("</br>");
	}

	protected get applicationPoolInfo(): IApllicationPoolShowInfo | null {
		if (!this.applicationPool) {
			return null;
		}
		return {
			state: this.getApplicationPoolState(),
			existProcesses: this.applicationPool.workerProcesses.length > 0,
			processesDescription: this.getWorkerProcessDescription(),
			type: ApplicationPoolStateMap[this.applicationPool.state] || "",
			effect: this.applicationPool.workerProcesses.length > 0 ? "dark" : "plain",
		};
	}

	protected get webAppDetails(): Array<{property: string, value: string}> {
		return [
			{ property: "physicalPath", value: this.app.physicalPath },
			{ property: "applicationPoolName", value: this.app.applicationPoolName },
			{ property: "dbConnectionString", value: this.app.dbConnectionString! || "" },
			{ property: "redisConnectionString", value: this.app.redisConnectionString! || "" },
		];
	}

}

</script>

<style lang="less">

.site-console-panel {
	
    display: flex;
    justify-content: space-between;
    align-items: baseline;
}

.right-cnt {
}

.application-processes-description {
	font-family: monospace;
}



body {
    padding: 0;
    margin: 0
}

.app {
    padding-top: 40px
}

.header-panel {
    position: fixed;
    width: 100%;
    top: 0;
    z-index: 2000;
    height: 40px;
    border-bottom: solid 1px #eee;
    box-shadow: 0 0 5px 0px #aaa;
}

.header-panel .el-input__inner {
    border-radius: 0;
    border-bottom: none;
    border-top: none;
}

.apps-list-cnt {
    padding: 10px;
    background-color: #fff;
    box-sizing: border-box;
    height: calc(100vh - 40px);
    overflow: auto
}

.web-app-cnt {
    padding: 5px 10px;
    margin-bottom: 10px;
    width: 100%;
    display: block;
    border: solid 1px #eee;
    box-shadow: 0 0 10px -5px #777;
    box-sizing: border-box
}

.web-app-cnt:hover {
    box-shadow: 0 0 10px -5px red;
    border-color: #11d5ff33;
    background-color: #ff000007
}

.app-control-panel {
    display: flex;
    flex-direction: column;
    justify-content: space-between
}

@media only screen and (min-width: 700px) {
    .app-control-panel {
        flex-direction:row
    }
}

.app-wrap-cnt {
    margin-top: 7px;
    display: block;
    padding: 5px;
    border: solid 1px #ccc;
    border-radius: 4px;
    background-color: #fff
}

.app-console-panel {
    margin-bottom: 7px
}

.code-editor-cnt {
    border: solid 1px #ddd
}

.app-caption {
    font-size: 28px;
    font-family: monospace
}

.app-property-name {
    font-family: sans-serif;
    font-weight: 700
}

.app-property-value {
    font-family: monospace
}

.app-property-value .cell {
    word-break: break-word
}

::-webkit-scrollbar {
    width: 10px;
    height: 10px
}

::-webkit-scrollbar-track {
    background: #f1f1f1!important
}

::-webkit-scrollbar-thumb {
    background: #bbb!important
}

::-webkit-scrollbar-thumb:hover {
    background: #888!important
}

</style>
