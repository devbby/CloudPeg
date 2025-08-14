export class ProcessingInfo{
    constructor(item: any, hardwareAcceleration: boolean) {
        this.file = item;
        this.hwAcceleration = hardwareAcceleration;
    }


    file: any
    hwAcceleration: boolean
}