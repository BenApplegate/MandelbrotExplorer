extern "C"{
    __declspec( dllexport ) int* runMandelbrot(int _width, int _height, double scale, double offsetX, double offsetY, int iterations, int threshold);
    __declspec( dllexport ) void clearMem(int* ptr);
}

__global__ void mandelKernel(int* data, int _width, int _height, double scale, double offsetX, double offsetY, int iterations, int threshold){

    int strideY = gridDim.x;
    int strideX = blockDim.x;
    int startY = blockIdx.x;
    int startX = threadIdx.x;

    for(int y = startY; y < _height; y += strideY){
        for(int x = startX; x < _width; x += strideX){
            
            double cReal, cImaginary;
            
            int tempX = x;
            int tempY = y;
            
            tempX -= (_width / 2);
            tempY -= (_height / 2);
            
            cReal = (tempX / (_width / scale)) + offsetX;
            cImaginary = (tempY / (_width / scale)) + offsetY;
            
            double zReal = 0;
            double zImaginary = 0;
            
            bool escapes = false;
            int i = 0;
            while(i < iterations && !escapes){
                double oldZreal = zReal;
                zReal = ((zReal * zReal) - (zImaginary * zImaginary)) + cReal;
                zImaginary = (2 * oldZreal * zImaginary) + cImaginary;
                
                if((zReal * zReal)+(zImaginary * zImaginary) > threshold * threshold){
                    escapes = true;
                    data[(y * _width) + x] = i;
                }
                ++i;
            }
            if(!escapes){
                data[(y * _width) + x] = -1;
            }
        }
    }
    
    return;
}

int* runMandelbrot(int _width, int _height, double scale, double offsetX, double offsetY, int iterations, int threshold){

    int* host_data = new int[_width * _height];
    int* device_data;
    cudaMalloc(&device_data, _width * _height * sizeof(int));

    mandelKernel<<<64, 1024>>>(device_data, _width, _height, scale, offsetX, offsetY, iterations, threshold);

    cudaMemcpy(host_data, device_data, _width * _height * sizeof(int), cudaMemcpyDeviceToHost);
    cudaFree(device_data);
    
    return host_data;
}

void clearMem(int* ptr){

    delete[] ptr;

}