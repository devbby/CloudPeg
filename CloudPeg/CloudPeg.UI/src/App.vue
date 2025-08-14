<script  lang="ts">

import {  defineComponent, inject} from "vue";
import {HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel} from "@microsoft/signalr";
import {ProcessingInfo} from "../../wwwroot/js/Models.ts";
 
export default defineComponent({
   setup()  {
 
  },

  components: {},

  data():{
    request: string,
    currentView: string,
    selectedItems: any[],
    selectedItem: ProcessingInfo | undefined
  }{
    return {
      request: "/fs",
      selectedItems: [],
      currentView: 'main2',
      selectedItem: undefined
    }
  },

  async mounted(){

    const connection = new HubConnectionBuilder()
        .withUrl("/VideoProcessor")
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build();

    // connection.onclose(error => {
    //   console.assert(connection.state === HubConnectionState.Disconnected);
    //   console.log('Connection closed due to error. Try refreshing this page to restart the connection', error);
    // });
    //
    // connection.onreconnecting(error => {
    //   console.assert(connection.state === HubConnectionState.Reconnecting);
    //   console.log('Connection lost due to error. Reconnecting.', error);
    // });
    //
    //
    connection.on('VideoProcessorConnected', res => {
      console.log("VideoProcessor connected! Ready ");
    });

    connection.on('videoprocessorstatusnotified', params => {
      console.log(params);
    });
    //
    // connection.onreconnected(connectionId => {
    //   console.assert(connection.state === HubConnectionState.Connected);
    //   console.log('Connection reestablished. Connected with connectionId', connectionId);
    // });

    await connection.start().then(async()=>{
      await connection.send("ConnectToVideoProcessor")

    })
     
     
  },

  computed:{

  },

  methods:{

    handleSelectButton(){

      // show select button
      // this.active: true,
      // // allow multiple selection
      // multiple: true,
      // // handle click event
      // click: (items:any, event:any) => {
      //   if (!items.length) {
      //     alert('No item selected');
      //     return;
      //   }
      //
      //   for (const item of items) {
      //     alert('Selected: ' + item.path);
      //   }
      //   console.log(items, event);
      // }
    },
    
    onResourceSelected(items: any) {
      this.selectedItem = new ProcessingInfo(items[0], true);
      console.log(items);
    },
    
    async onProcessFile(){
      if(this.selectedItem !== undefined){
        await this.processFile(this.selectedItem.file.path, this.selectedItem.hwAcceleration, )
      }
    },

   
    async processFile(filePath: string, enableHardwareAcceleration: boolean){

      let data ={
        filePath,
        enableHardwareAcceleration
      }
      return await fetch(`/Home/Process`,{method: 'POST',headers: { 'Content-Type': 'application/json' }, credentials: 'include', body: JSON.stringify(data)})
          .then(async (response) => {
            if (response.ok) {
              return response.json();
            }
            let errorBody = "";
            await response.text().then(body => {
              errorBody = body;
            });
            throw new Error(errorBody);
          }).then(async value => {

            return value;
          }).catch(error => {
            alert(error);
          });
    },
   
 
  }
});
 
</script>

<template>
  <h2>CloudPeg</h2>
  <div v-if="selectedItem"> 
     <span> 
      <span>{{selectedItem.file?.basename}} </span>
      <input class="ms-2" type="checkbox" v-model="selectedItem.hwAcceleration"/>
      
    </span>
   
  </div>
  
  <div class="container" style="max-height: 300px; height: 300px">
    <div class="row">
      <div class="col">
        <template v-if="selectedItem">
          <button v-on:click="onProcessFile" class="btn btn-secondary"> Process </button>
        </template>
      </div>
    </div>
    
  </div>
  
  <div id="fs-container">
    <vue-finder
        id='my_vuefinder' v-on:select="onResourceSelected" :url="request" :request="request" theme="light" :select-button="handleSelectButton"

    />
  </div>
  
</template>

<style scoped> 

#fs-container {   
}
</style>
