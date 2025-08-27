<script  lang="ts">

import {  defineComponent, inject} from "vue";
import {HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel} from "@microsoft/signalr";
import {ProcessingInfo, QueueItem} from "../../wwwroot/js/Models.ts";
import ProcessingStatus from "@/components/ProcessingStatus.vue";
 
export default defineComponent({
   
  setup()  {
 
  },

  components: {ProcessingStatus},

  data():{
    request: string,
    currentView: string,
    selectedItems: any[],
    selectedItem: ProcessingInfo | undefined,
    queue: QueueItem[] | undefined,
    templates: any,
    selectedTemplate: any
  }{
    return {
      request: "/fs",
      selectedItems: [],
      currentView: 'main2',
      selectedItem: undefined,
      queue: undefined,
      templates: undefined,
      selectedTemplate: undefined
    }
  },

  async mounted(){

    this.templates = await this.getConversionTemplates();
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
      
      this.queue = params;
      
      console.log(this.queue);
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
        await this.processFile(this.selectedItem.file.path, this.selectedTemplate, )
      }
    },
    
    
    async onProcessingCancelled(item: QueueItem){
      
      
      await this.cancelProcessing(item);
      
    },

    async cancelProcessing(item: QueueItem){

       
      return await fetch(`/Home/CancelProcessing`,
          {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }, 
                credentials: 'include', 
                body: JSON.stringify(item)})
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
   
    async processFile(filePath: string, template: any){

      let data ={
        filePath,
        template
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
   
    async getConversionTemplates(){
      return await fetch(`/Home/GetConversionTemplates/`,
          {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' }, 
            credentials: 'include' 
          })
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
      
      
    }
 
  }
});
 
</script>

<template>
  <h2 class="p-3">
    <span class="bi bi-cloud"></span>
    CloudPeg
  </h2>
  
  <div class="container-fluid">
    <div class="row">
      <div class="col-6">
        <small class="text-muted ">Selection</small>
        <template v-if="selectedItem"> 
          <div>
            <span> 
              <span>{{selectedItem.file?.basename}} </span>

<!--              <input class="ms-2" type="checkbox" v-model="selectedItem.hwAcceleration"/>-->
            </span>
          </div>
        </template>
      </div>
      
      <div class="col-6">
        <small class="text-muted ">QUEUE</small>
        
          <div >
            <div v-for="item in queue">
              <processing-status :status="item.status"></processing-status>
              <span class="ms-3">{{item.processRequest.resource.basename}}</span>
              <span v-if="item.status === 2" class="ms-3">{{item.processRequest.progress}} %</span>
              <span v-on:click="onProcessingCancelled(item)" v-if="item.status === 2" class="ms-3 link link-danger">Cancel</span>
            </div>
          </div>
      
      </div>
    </div>
  </div>

  
  <div class="container" style="max-height: 300px; height: 300px">
    <div class="row">
      <div class="col">

        
        
        <div v-if="selectedItem">
          <select class="me-3" v-model="selectedTemplate">
            <option v-for="template in templates" :value="template">{{template.name}}</option>
          </select>
          
          <button v-on:click="onProcessFile" class="btn btn-secondary"> Process </button>
        </div>
      </div>
    </div>
    
  </div>
  
  <div id="fs-container">
    <vue-finder
        id='my_vuefinder' v-on:select="onResourceSelected" :url="request" :request="request" theme="dark" :select-button="handleSelectButton"

    />
  </div>
    
</template>

<style scoped> 

#fs-container {   
}
</style>
