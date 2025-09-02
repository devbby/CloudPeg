<script  lang="ts">

import {  defineComponent, inject} from "vue";
import {HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel} from "@microsoft/signalr";
import {MediaInfo, ProcessingInfo, QueueItem} from "../../wwwroot/js/Models.ts";
import ProcessingStatus from "@/components/ProcessingStatus.vue";
import moment from "moment";

 
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
    selectedQueueItem: QueueItem | undefined,
    templates: any,
    selectedTemplate: any,
    isSample: boolean,
    modalProcessInfo: any,
    supportedCodecs: any,
  }{
    return {
      request: "/fs",
      selectedItems: [],
      currentView: 'main2',
      selectedItem: undefined,
      queue: undefined,
      templates: undefined,
      isSample: false,
      selectedTemplate: undefined,
      modalProcessInfo: undefined,
      selectedQueueItem: undefined,
      supportedCodecs: undefined,
    }
  },

  async mounted(){
    this.supportedCodecs = await this.getSupportedCodecs();
    this.templates = await this.getConversionTemplates();
    const connection = new HubConnectionBuilder()
        .withUrl("/VideoProcessor")
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build();

    this.modalProcessInfo =  this.$refs.modalProcessInfo;

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
      
      // console.log(this.queue);
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
    
    async onResourceSelected(items: any) {
      
      this.selectedItem = new ProcessingInfo(items[0], true);
      
      try {
        let mediaInfo = await this.getMediaInfo(this.selectedItem) ;
        if(mediaInfo !== undefined) {
          this.selectedItem.setMediaInfo(mediaInfo);
        }
      }catch (e){
        console.log(e)
      }
      
      
      console.log(this.selectedItem);
    },
    
    async onProcessFile(){
      if(this.selectedItem !== undefined){
        let videoStreams: number[] = [];
        for (const element of $("input.video-toggle:checked")) {
          let i = $(element).attr("index");
          if(i !== undefined)
            videoStreams.push( parseInt(i) );
        }
        
        let audioStreams: number[] = [];
        for (const element of $("input.audio-toggle:checked")) {
          let i = $(element).attr("index");
          if(i !== undefined)
            audioStreams.push( parseInt(i));
        }
        
        let subtitleStreams: number[] = [];
        for (const element of $("input.sub-toggle:checked")) {
          let i = $(element).attr("index");
          if(i !== undefined)
            subtitleStreams.push( parseInt(i));
        }
        
        
        await this.processFile(this.selectedItem.file.path, this.selectedTemplate,videoStreams, 
            audioStreams, subtitleStreams, this.isSample )
      }
    },

    onShowProcessingInfo(item: QueueItem){

      this.selectedQueueItem = item;
      
      $(this.modalProcessInfo).modal("show")
      
    },
    
    async onProcessingCancelled(item: QueueItem){
      
      await this.cancelProcessing(item);
      
    },

    async onRemoveEnqueued(item: QueueItem){

      await this.removeEnqueued(item);
      
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
     
    async removeEnqueued(item: QueueItem){
 
      return await fetch(`/Home/RemoveEnqueuedItem`,
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
   
    async processFile(filePath: string, template: any,  videoStreams: number[], audioStreams: number[], subtitleStreams: number[], isSample: boolean){

      let data ={
        filePath,
        template,
        videoStreams,
        audioStreams,
        subtitleStreams,
        isSample
      }
      return await fetch(`/Home/Process`,
          {
            method: 'POST',headers: { 'Content-Type': 'application/json' }, credentials: 'include', body: JSON.stringify(data)})
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
      
      
    },

    async getMediaInfo(item: ProcessingInfo) : Promise<MediaInfo | undefined  >  {
      
      let data = {
        filePath: item.file.path
      }
      return fetch(`/Home/GetMediaInfo`,
          {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include',
            body: JSON.stringify(data)})
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

            let info = new MediaInfo(
                value.primaryVideoIndex, 
                value.primaryAudioIndex,
                value.primarySubtitleIndex,
                value.videoStreams, 
                value.audioStreams, 
                value.subtitleStreams);
            return info;
          }).catch(error => {
            alert(error);
            return undefined;
          });
    },
    
    async getSupportedCodecs(){
       
      return fetch(`/Home/GetSupportedCodecs`,
          {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include'}
          ).then(async (response) => {
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
            return undefined;
          });
    },

    getFormattedDateTime(date: string){
      
      // return moment(date).format("DD/MM/YYYY HH:mm:ss")
      return moment(date).format("HH:mm")
    },

    getElapsedSinceProcessingStart(item: any){

      let startTime  = moment(item?.processRequest.processingStarted);
      const duration = moment.duration(moment().diff(startTime));
      const minutes = Math.floor(duration.asMinutes());
      const seconds = duration.seconds();

      const formattedDuration = `${minutes}m ${seconds}s`;
      return formattedDuration;
      
    },

    
  }
});
 
</script>

<template>
  <h2 class="p-3">
    <span class="bi bi-cloud"></span>
    CloudPeg
  </h2>
  
  <div id="fs-container mb-3">
    <vue-finder
        id='my_vuefinder' v-on:select="onResourceSelected" :url="request" :request="request" theme="dark" :select-button="handleSelectButton"
    />
  </div>
  
  <div class="container-fluid">
    <div class="row">
      <div class="col-5">
        <small class="text-muted text-uppercase">Target File</small>
        <template v-if="selectedItem"> 
          <div>
            <span v-if="selectedItem?.mediaInfo === undefined || selectedItem?.mediaInfo === null && selectedItem.file.type !== 'file'"> 
              <span>{{selectedItem.file?.basename}} </span>
            </span>
          </div>

          <div v-if="selectedItem?.mediaInfo !== undefined && selectedItem?.mediaInfo !== null && selectedItem.file.type === 'file'">

            <div class="card">
              <div class="card-content">
                <div class="card-header">

                  <span>{{selectedItem.file?.basename}} </span>
                  
                </div>
                <div class="card-body">
                  
                  <div>
                    <span class="bi bi-film me-2"></span> Video stream
                    <div class="ms-4">
                      <div >
                        <span><small class="text-muted">CODEC</small> {{ selectedItem?.mediaInfo?.getPrimaryVideoStream()?.codecName}} </span>
                      </div>
                      <div>
                        <span><small class="text-muted">FPS</small> {{ selectedItem?.mediaInfo?.getPrimaryVideoStream()?.frameRate }} </span>
                      </div>
                      <div>
                        <span><small class="text-muted">SIZE</small> {{ selectedItem?.mediaInfo?.getPrimaryVideoStream()?.width}}x{{ selectedItem?.mediaInfo?.getPrimaryVideoStream()?.height}} </span>
                      </div>
                      <div>
                        <span><small class="text-muted">LANGUAGE</small> {{ selectedItem?.mediaInfo?.getPrimaryVideoStream()?.language}} </span>
                      </div>
                      <div>
                        <span><small class="text-muted">VIDEO STREAMS</small> {{ selectedItem?.mediaInfo?.videoStreams?.length}} </span>
                      
                        <div>
                          <template v-for="video in selectedItem?.mediaInfo?.videoStreams" >
                            <input type="checkbox" class="video-toggle btn-check" :index="video.index" :id="'btn-video-check-'+video.index" checked :value="video" autocomplete="off">
                            <label class="btn btn-sm ms-1" :for="'btn-video-check-'+video.index">
                              <span class=" bi bi-film me-2"></span>
                              {{ video.index }}. {{ video.language }}</label>
                          </template>
                        </div>
                        
                      </div>
                    </div>
                  </div>

                  <div class="mt-2" >
                    <span class="bi bi-headphones me-2"></span> Audio stream
                    <div class="ms-4">
                      <div>
                        <span><small class="text-muted">CODEC</small> {{ selectedItem?.mediaInfo?.getPrimaryAudioStream()?.codecName  }} </span>
                      </div>
                      <div>
                        <span><small class="text-muted">CHANNELS</small> {{ selectedItem?.mediaInfo?.getPrimaryAudioStream()?.channels }}  {{ selectedItem?.mediaInfo?.getPrimaryAudioStream()?.channelLayout }}</span>
                      </div>
                      <div>
                        <span><small class="text-muted">AUDIO STREAMS</small> {{ selectedItem?.mediaInfo?.audioStreams?.length}} </span>
                        <div>
                          <template v-for="audio in selectedItem?.mediaInfo?.audioStreams" >
                            <input type="checkbox" class=" audio-toggle btn-check" :index="audio.index"  :id="'btn-audio-check-'+audio.index" checked :value="audio" autocomplete="off">
                            <label class="btn btn-sm ms-1" :for="'btn-audio-check-'+audio.index">
                              <span class=" bi bi-headphones me-2 "></span>
                              {{ audio.index }}. {{ audio.language }}</label>
                          </template>
                        </div>
                      </div>
                    </div>
                  </div>

                  <div  class="mt-2">
                    <span class="bi bi-badge-cc-fill me-2"></span> Subtitle streams
                    <div class="ms-4">
                      <div>
                        <span><small class="text-muted">CODEC</small> {{ selectedItem?.mediaInfo?.getPrimarySubtitleCodec() }} </span>
                      </div>
                      <div>
                        <span><small class="text-muted">LANGUAGE</small> {{ selectedItem?.mediaInfo?.getPrimarySubtitleLanguage() }}   </span>
                      </div>
                      <div>
                        <span><small class="text-muted">SUBTITLE STREAMS</small> {{ selectedItem?.mediaInfo?.getMediaInfoSubStreamCount() }} </span>
                        <div>
                          <template v-for="sub in selectedItem?.mediaInfo?.subtitleStreams" >
                            <input type="checkbox" class="sub-toggle btn-check" :index="sub.index" :id="'btn-sub-check-'+sub.index" checked :value="sub" autocomplete="off">
                            <label class="btn btn-sm ms-1" :for="'btn-sub-check-'+sub.index">
                              <span class=" bi bi-badge-cc-fill me-2"></span>
                              {{ sub.index }}. {{ sub.language }}</label>
                          </template>
                        </div>
                      </div>
                    </div>
                  </div>

                  
                  
                </div>
              </div>
            </div>
          </div>
        </template>
      </div>
      
      <div class="col-7">
        <small class="text-muted ">QUEUE</small>
        
          <div class="queue-container">
            <div class="ms-1" v-for="item in queue">
              <div class="card mb-2">
                <div class="card-content">
                  <div class="card-header">

                    <processing-status class="processing-status" v-on:click="onShowProcessingInfo(item)" :status="item.status"></processing-status>
                    <small class="ms-3 text-muted text-uppercase queue-item-file" >{{item.processRequest.resource.basename}}</small>


                    <span class="ms-3">{{getFormattedDateTime(item.created)}}</span>


                    <div v-if="item.status === 2" class="d-flex queue-item-progress-container">
                      <span class="me-3">{{item.processRequest.progress}} %</span>
                      <div  class="progress queue-item-progress-bar" role="progressbar" aria-label="Animated striped example" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100">
                        <div class="progress-bar progress-bar-striped progress-bar-animated " :style="{'width': item.processRequest.progress+'%'}"></div>
                      </div>
                      <span class="me-3 ms-3">{{getElapsedSinceProcessingStart(item) }}</span>
                      <span v-on:click="onProcessingCancelled(item)" v-if="item.status === 2" class="ms-3 link link-danger">Cancel</span>
                    </div>
                    <div v-if="item.status === 1" class="d-flex">
                      <span v-on:click="onRemoveEnqueued(item)" class="ms-3 link link-danger">
                        <span class="bi bi-trash-fill me-1"></span> Remove
                      </span>
                    </div>

                  </div>
                </div>
              </div>
              


            </div>
          </div>
      
      </div>
    </div>
  </div>
  
  <div class="container mt-2" style="max-height: 300px; height: 300px">
    <div class="row">
      <div class="col">
        <div v-if="selectedItem?.mediaInfo !== undefined && selectedItem?.mediaInfo !== null && selectedItem.file.type === 'file'">

          <div class="form-check">
            <input v-model="isSample" class="form-check-input" type="checkbox" value="" id="checkSample">
            <label class="form-check-label" for="checkSample">
              Sample
            </label>
          </div>
          
          <select class="me-3" v-model="selectedTemplate">
            <option v-for="template in templates" :value="template">{{template.name}}</option>
          </select>
          
          <button v-on:click="onProcessFile" class="btn btn-sm btn-primary"> Process </button>
        </div>
      </div>
    </div>
    
  </div>
   
  <div ref="modalProcessInfo" class="modal" tabindex="-1">
    <div class="modal-dialog" >
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title">
            <processing-status class="me-2" :status="selectedQueueItem?.status"></processing-status>
            {{selectedQueueItem?.processRequest.resource.basename}}
          </h5>
          <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
        </div>
        <div class="modal-body">
          <p> {{selectedQueueItem?.info}} </p>
        </div>
        <div class="modal-footer">
        </div>
      </div>
    </div>
  </div>

</template>

<style scoped>
  
  .queue-item-file{
    font-size: 0.8rem;
  }

  .queue-item-progress-container{
     flex-direction: row;
  }
  
  .queue-item-progress-bar{
       flex: 1 1 auto;
  }
   
  .processing-status{
    cursor: pointer;   
  }
  
  .modal-content{
    width: 682px;
  }
  
  .queue-container {
    max-height: 544px;
    overflow: scroll;    
  }
  
  #fs-container {   
  }
</style>
