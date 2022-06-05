function abcd1(_0xd7a12a) {
	return abcd2(_0xd7a12a, 0x36e455);
}
function abcd2(_0x5d3f51, _0x2da958) {
	if (!abcdx())
	return;
	var _0x17fe67 = 0x80000000;
	var _0x330f76 = 0x7fffffff;
	var _0x8c2ab2 = ~~(_0x5d3f51 / _0x17fe67);
	var _0x33045d = ~~(_0x2da958 / _0x17fe67);
	var _0xa5b0b1 = (_0x5d3f51 & _0x330f76);
	var _0x352369 = (_0x2da958 & _0x330f76);
	var _0x3f97b8 = (_0x8c2ab2 ^ _0x33045d);
	var _0x4307f8 = (_0xa5b0b1 ^ _0x352369);
	return ((_0x3f97b8 * _0x17fe67) + _0x4307f8);
}
function abcd3(_0x2a839f, _0x29778) {
	if (((_0x2a839f - 0x3e) < 0x0)) {
		var _0x47bf41 = _0x29778['substr'](_0x2a839f, 0x1);
		return _0x47bf41;
	}
	var _0x5aa264 = (_0x2a839f % 0x3e);
	var _0xf91ae6 = parseInt((_0x2a839f / 0x3e));
	return (abcd3(_0xf91ae6, _0x29778) + _0x29778['substr'](_0x5aa264, 0x1));
}
function abcd4(_0x2d1e33, _0x4ccbfe) {
	if (!abcdx())
	return;
	var _0x39c799 = _0x4ccbfe['split']('');
	var _0x368c45 = _0x4ccbfe['length'];
	for (var _0x2faa11 = 0x0; (_0x2faa11 < _0x2d1e33['length']); _0x2faa11++) {
		var _0x1e44d9 = parseInt(_0x2d1e33[_0x2faa11]);
		var _0x172ebc = _0x39c799[_0x1e44d9];
		var _0x939174 = _0x39c799[((_0x368c45 - 0x1) - _0x1e44d9)];
		_0x39c799[_0x1e44d9] = _0x939174;
		_0x39c799[((_0x368c45 - 0x1) - _0x1e44d9)] = _0x172ebc;
	}
	_0x4ccbfe = _0x39c799['join']('');
	return _0x4ccbfe;
}
function abcd5(_0x30fc22) {
	if (!abcdx())
	return;
	var _0x416034 = 0x0;
	var _0x39f6f7 = _0x30fc22['split']('');
	for (var _0x3a1139 = 0x0; (_0x3a1139 < _0x39f6f7['length']); _0x3a1139++) {
		_0x416034 += _0x39f6f7[_0x3a1139]['charCodeAt']();
	}
	var _0x33f508 = _0x30fc22['length'];
	var _0x51436b = (_0x416034 % _0x33f508);
	var _0xd1d5ab = [];
	for (var _0x3a1139 = _0x51436b; (_0x3a1139 < _0x33f508); _0x3a1139++) {
		_0xd1d5ab['push'](_0x39f6f7[_0x3a1139]);
	}
	for (var _0x3a1139 = 0x0; (_0x3a1139 < _0x51436b); _0x3a1139++) {
		_0xd1d5ab['push'](_0x39f6f7[_0x3a1139]);
	}
	return _0xd1d5ab['join']('');
}
function abcdu(_0x32b9a8) {
	var _0x4ba9ab = -0x1e0;
	var _0x1ce339 = new Date()['getTimezoneOffset']();
	var _0x2ab335 = (_0x4ba9ab - _0x1ce339);
	return ((_0x32b9a8['getTime']() / 0x3e8) + (_0x2ab335 * 0x3c));
}
function abcdx() {
	if (navigator['webdriver'])
	return ![];
	if (document['$cdc_asdjflasutopfhvcZLmcfl_'])
	return ![];
	if (/PhantomJS/['test'](window['navigator']['userAgent']))
	return ![];
	if (window['callPhantom'] || window['_phantom'])
	return ![];
	return !![];
}
$(function () {
	setTimeout(function () {window['hdm1113'] = !![];}, 0x3e8);
	function _0x156205(_0x5326c0) {
		if ((_0x5326c0['pageX'] > 0x0) && abcdx() && window['hdm1113']) {
			var _0x4b9009 = rndnum['split']('.')[0x0];
			var _0x37348b = abcd1(parseInt(_0x4b9009));
			var _0x307a46 = (_0x37348b + '')['split']('');
			var _0x2f4e03 = $('#starttime')['val']() || window['initstime'];
			var _0x17071c = abcdu(new Date(_0x2f4e03['replace'](new RegExp('-', 'gm'), '/')));
			var _0x12e25a = (_0x17071c + '');
			if (((_0x17071c % 0xa) > 0x0))
			_0x12e25a = _0x12e25a['split']('')['reverse']()['join']('');
			var _0x24bc24 = parseInt((_0x12e25a + '89123'));
			var _0x307a46 = ((_0x24bc24 + '') + (_0x37348b + ''))['split']('');
			var _0xd36323 = abcd4(_0x307a46, 'kgESOLJUbB2fCteoQdYmXvF8j9IZs3K0i6w75VcDnG14WAyaxNqPuRlpTHMrhz');
			var _0xc23193 = ((_0x24bc24 + _0x37348b) + parseInt(activityId));
			jqParam = abcd3(_0xc23193, _0xd36323);
			var _0x16feb7 = abcd5(jqParam);
			jqParam = _0x16feb7;
			$(document)['unbind']('mousemove', _0x156205);
		}
	}
	$(document)['bind']('mousemove', _0x156205);
});
