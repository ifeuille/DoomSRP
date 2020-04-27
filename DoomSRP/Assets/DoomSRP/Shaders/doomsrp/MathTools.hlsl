#ifndef DOOMSRP_MATHTOOLS_H
#define DOOMSRP_MATHTOOLS_H
//判断是这一位是否为1,bitValue 1<<bitNum
bool Bit_And_Single(uint value, uint bitValue)
{
	return (value / bitValue) % 2 == 1;
}

uint uint_bitfieldExtract(uint val, int off, int size) {
	uint mask = uint((1 << size) - 1);
	return uint(val >> off) & mask;
}

//off注意符号位问题,这个函数可以避免unity优化为bitfieldExtract
//但如果符号位为1,该方法结果错误
uint int_bitfieldExtract(uint val, int off, int size)
{
	//uint v31 = val & 0x80000000;//提取最高位
	//int ival = val & 0x7fffffff;//除了符号位
	int mask = int((1 << size) - 1);
	//val = val >> off;
	//val = val & mask;
	//return val;
	int val1 = int(val >> off);
	return uint(uint(val1)& mask);
	/*int mask = int((1 << size) - 1);
	return uint(((int)val >> off) & mask);*/
}


#endif