export class ProcessingInfo{
    constructor(item: any, hardwareAcceleration: boolean) {
        this.file = item;
        this.hwAcceleration = hardwareAcceleration;
    }

    file: any
    hwAcceleration: boolean
}

export class ProcessRequest{
    
    progress: number;
    
    resource: {
        basename: string;
    }
    
    constructor(resource: any, progress: number) {
        this.resource = resource;
        this.progress = progress;
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