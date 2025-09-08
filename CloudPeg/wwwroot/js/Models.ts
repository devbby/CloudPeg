export class ProcessingInfo{
    mediaInfo: MediaInfo | undefined;
    constructor(item: any, hardwareAcceleration: boolean) {
        this.file = item;
        this.hwAcceleration = hardwareAcceleration;
    }

    file: any
    hwAcceleration: boolean
    
    setMediaInfo(info: MediaInfo){
        this.mediaInfo = info;        
    }
}

export class ProcessRequest{
    
    id: string;
    
    progress: number;
    eta: string;
    resource: {
        basename: string;
    }

    processingStarted: string;
    processingEnded: string;
    isSample: boolean;
    
    constructor(id: string, resource: any, progress: number, processingStarted: string, processingEnded: string, isSample: boolean, eta: string) {
        this.id = id;
        this.resource = resource;
        this.progress = progress;
        this.processingStarted = processingStarted;
        this.processingEnded = processingEnded;
        this.isSample = isSample;
        this.eta = eta;
    }
}

export class QueueItem {
    
    status: number
    created: any
    processRequest: ProcessRequest;
    info: string;
    constructor(status: number, created: any, processRequest: ProcessRequest,info: string ) {
        this.status = status;
        this.created = created;
        this.processRequest = processRequest;
        this.info = info;
    }
}

export class MediaInfo {
    primaryVideoIndex : number;
    primaryAudioIndex : number;
    primarySubtitleIndex : number;
    videoStreams: any[];
    audioStreams: any[];
    subtitleStreams: any[];
    
    
    constructor(primaryVideoIndex: number, primaryAudioIndex: number, primarySubtitleIndex: number,
                videoStreams: any[], audioStreams: any[], subtitleStreams: any[]) {
        this.primaryVideoIndex = primaryVideoIndex;
        this.primaryAudioIndex = primaryAudioIndex;
        this.primarySubtitleIndex = primarySubtitleIndex;
        this.videoStreams = videoStreams;
        this.audioStreams = audioStreams;
        this.subtitleStreams = subtitleStreams;

    }

    getPrimaryVideoStream(){

        let index = this.primaryVideoIndex;
        if(this.videoStreams === undefined || this.videoStreams.length < 1){
            return undefined;
        }

        if(this.videoStreams[index] === undefined || index === -1){
            index = 0;
        }

        return this.videoStreams[index];
    }

    getPrimaryAudioStream(){

        let index = this.primaryVideoIndex;
        if(this.audioStreams === undefined || this.audioStreams.length < 1){
            return undefined;
        }

        if(this.audioStreams[index] === undefined || index === -1){
            index = 0;
        }

        return this.audioStreams[index];
    }
    
    getPrimarySubtitleCodec(){

        let index = this.primarySubtitleIndex;
        if(this.subtitleStreams === undefined || this.subtitleStreams.length < 1){
            return undefined;
        }

        if(this.subtitleStreams[index] === undefined || index === -1){
            index = 0;
        }

        return this.subtitleStreams[index].codecName;
    }
    
    getPrimarySubtitleLanguage(){
        let index = this.primarySubtitleIndex;
        
        if(this.subtitleStreams === undefined || this.subtitleStreams.length < 1){
            return undefined;
        }
        
        if(this.subtitleStreams[index] === undefined || index === -1){
            index = 0;
        }
        
        return this.subtitleStreams[index].language;
    }
    getMediaInfoSubStreamCount(){
        return this.subtitleStreams?.length ?? 0;
    }
    
}