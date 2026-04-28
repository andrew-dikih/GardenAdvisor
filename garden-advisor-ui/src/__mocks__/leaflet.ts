const L = {
  icon: jest.fn(() => ({})),
  Marker: {
    prototype: {
      options: { icon: null },
    },
  },
};

export default L;
export const icon = L.icon;
