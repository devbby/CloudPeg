<script  lang="ts">

import {  defineComponent, inject} from "vue";
import {HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel} from "@microsoft/signalr";
export default defineComponent({
   setup()  {
 
  },

  components: {},

  data():{
    request: string,
    currentView: string,
    selectedItems: any[]
  }{
    return {
      request: "/fs",
      selectedItems: [],
      currentView: 'main2',
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
      this.selectedItems = items;
      console.log(items);
    },
    
    async onProcessFile(){
      
      await this.processFile(this.selectedItems.map(x=>x.path))
    },

   
    async processFile(filePaths: string[]){

      let data ={
        filePaths
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
  <div v-for="item in selectedItems">{{item.basename}}</div>
  
  <div class="container" style="max-height: 300px; height: 300px">
    <div class="row">
      <div class="col">
        <template v-if="selectedItems.length > 0">
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
