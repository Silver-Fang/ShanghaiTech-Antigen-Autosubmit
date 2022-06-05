function 取62基数子串(基数, 字符串) {
	if (基数 < 62) 
		return 字符串.substr(基数, 1);
	return 取62基数子串(parseInt(基数 / 62), 字符串) + 字符串.substr(基数 % 62, 1);
}
function GetJqparam(rndnum, starttime, activityId) {
	var 操作数1 = parseInt(rndnum.split('.')[0]) ^ 0x36e455;
	var 操作数2 = new Date(starttime.replace(new RegExp('-', 'gm'), '/')).getTime() / 1000 - (0x1e0 + new Date().getTimezoneOffset()) * 60;
	var 字符串 = 操作数2 + '';
	if (操作数2 % 0xa)
		字符串 = 字符串.split('').reverse().join('');
	操作数2 = parseInt(字符串 + '89123');
	var 输入字符 = (操作数2 + '' + 操作数1 + '').split('');
	字符串 = 'kgESOLJUbB2fCteoQdYmXvF8j9IZs3K0i6w75VcDnG14WAyaxNqPuRlpTHMrhz';
	var 输出字符 = 字符串.split('');
	var 字符串长度 = 字符串.length;
	for (var _0x107cfb = 0; _0x107cfb < 输入字符.length; _0x107cfb++) {
		var _0x410c33 = parseInt(输入字符[_0x107cfb]);
		字符串 = 输出字符[_0x410c33];
		输出字符[_0x410c33] = 输出字符[字符串长度 - 1 - _0x410c33];
		输出字符[字符串长度 - 1 - _0x410c33] = 字符串;
	}
	字符串 = 取62基数子串(操作数2 + 操作数1 + parseInt(activityId), 输出字符.join(''));
	操作数1 = 0;
	输入字符 = 字符串.split('');
	for (操作数2 = 0; 操作数2 < 输入字符.length; 操作数2++) {
		操作数1 += 输入字符[操作数2].charCodeAt();
	}
	字符串长度 = 字符串.length;
	操作数1 %= 字符串长度;
	输出字符 = [];
	for (操作数2 = 操作数1; (操作数2 < 字符串长度); 操作数2++) {
		输出字符.push(输入字符[操作数2]);
	}
	for (操作数2 = 0;操作数2 < 操作数1; 操作数2++) {
		输出字符.push(输入字符[操作数2]);
	}
	return 输出字符.join('');
}
var jqParam = GetJqparam("997740503.61715449", "2022/5/29 20:53:38", 164395883);
jqParam;